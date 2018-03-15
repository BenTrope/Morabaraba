﻿module Morabaraba.Game
open DataSets
open Drawing

// The following functions are the functions passed into UPDATEBOARD. They determine how the board will be updated.
// - (? Ernest unsure) means I was wasn't completely sure what is here year
// - this refactor is just for building a better understanding of what we did

//---------------------------------------------------------------------------------
// UPDATE FUNCTIONS: INSERT/REMOVE/UPDATE board
//---------------------------------------------------------------------------------

/// <summary>
/// this function is passed into updateboard (custom List.mapi), operates on each element
/// </summary>
/// <param name="player">Which player is using this (? Ernest unsure)</param>
/// <param name="coord">The co-ordinates we are using</param>
/// <param name="board">The board we are using</param>
/// <param name="index">The index of the board element</param>
/// <param name="valu">(? Ernest unsure)</param>
let insertcow (player: Player) (coord: int) (board : int list) (index : int) (valu: Player): Player = //this function is passed into updateboard (custom List.mapi), operates on each element
    match index=coord with
    | true -> player // coord ofType in. (x+y)
    |_ -> valu

/// <summary>
/// Removing a cow from a place on the board
/// </summary>
/// <param name="player">Which player is using this (? Ernest unsure)</param>
/// <param name="coord">The co-ordinates we are using</param>
/// <param name="board">The board we are using</param>
/// <param name="index">The index of the board element</param>
/// <param name="valu">(? Ernest unsure)</param>
let removecow (player: Player) (coord: int) (board : int list) (index : int) (valu: Player): Player = 
    match (index = coord) with
    | true -> Neither //coord ofType in. (x+y)
    |_ -> valu

/// <summary>
/// Update the board at a specific coordinate
/// </summary>
/// <param name="f">(? Ernest unsure)</param>
/// <param name="player">Which player is using this (? Ernest unsure)</param>
/// <param name="coord">The co-ordinates we are using</param>
/// <param name="board">The board we are using</param>
let updateboard (f : Player -> int -> int list -> int -> Player -> Player) (player: Player) (coord: int) (board : int list) : int list =      //Similar to mapi, altered for game purposes
    let rec innerF (lst : int list) (inc : int) (len : int) (ans : int list) : int list =    // inc used as length at first call, and list incerment in recursion 
        match (inc - len) with
        |0 -> ans
        |a ->
            let fInner = f player coord lst inc (swapIntToPlayer lst.[(inc)])
            innerF lst (inc+1) len (List.append ans [(swapPlayerToInt fInner)])
    innerF board 0 board.Length []

/// <summary>
/// Works out if we have a mill using the list of mills
/// </summary>
/// <param name="mills">The list of mills we can search through</param>
/// <param name="board">The board we are using</param>
/// <param name="spot">The co-ordinates we are using</param>
/// <param name="player">The current player</param>
let ismill (board : int list) (spot : int list) (player : int): bool =
    let getMillValue =
        fun (m:Mill) ->
            (board.[m.PointA.x + m.PointA.y]=player)&&((board.[m.PointA.x + m.PointA.y]=board.[m.PointB.x + m.PointB.y])&&(board.[m.PointC.x + m.PointC.y]=board.[m.PointB.x + m.PointB.y]))
    let rec getTheMills = 
        fun (sspot : int list) (allThem : Mill list) (b : Mill list) ->
            match b with
            | [] -> allThem
            | head::tail ->
                match ((head.PointA.x=sspot.[0])&&(head.PointA.y=sspot.[1]))||((head.PointB.x=sspot.[0])&&(head.PointB.y=sspot.[1]))||((head.PointC.x=sspot.[0])&&(head.PointC.y=sspot.[1])) with
                | true -> getTheMills sspot (head::allThem) tail
                | false -> getTheMills sspot allThem tail
    let checkEachOption =
        fun (m: Mill) (spot: int list) ->
            match ((m.PointA.x+m.PointA.y=spot.[0]+spot.[1])||(m.PointB.x+m.PointB.y=spot.[0]+spot.[1])||(m.PointC.x+m.PointC.y=spot.[0]+spot.[1])) with
            | true ->
                let a, b, c, checkEquals = board.[m.PointA.x+m.PointA.y], board.[m.PointB.x+m.PointB.y], board.[m.PointC.x+m.PointC.y], board.[spot.[0]+spot.[1]]
                match a=checkEquals,b=checkEquals,c=checkEquals with
                | true, true, true -> true
                | _ -> false
            | _ -> false
    let chosenMills = getTheMills spot [] mills
    let rec eachChoice =
        fun (cm: Mill list) (spooot: int list) ->
            match cm with
            | [] -> false
            | head::tail ->
                match checkEachOption head spot with
                | true -> true
                | false -> eachChoice tail spot
    eachChoice chosenMills spot

//---------------------------------------------------------------------------------
// MAIN FUNCTIONS: PLACE, MOVE FLY
//---------------------------------------------------------------------------------

/// <summary>
/// Work out what is in a position on the board and see if it is what we expect
/// </summary>
/// <param name="board">The board we are using</param>
/// <param name="coords">The co-ordinates we are using</param>
/// <param name="expect">What we are expecting</param>
let inAct =
    fun (board : int list) (coords : int list) (expect : Player) -> // 0 empty, 1 player, 2 other player
        board.[coords.[0] + coords.[1]]=(swapPlayerToInt expect)

/// <summary>
/// The main game loop, more is eplained inside
/// </summary>
/// <param name="player">The current player</param>
/// <param name="board">The board we are using</param>
/// <param name="sentence">The sentence which describes the current play that's running</param>
/// <param name="expect">What we are expecting</param>
let rec interaction (player : Player) (board : int list) (sentence : string) (expect : Player) : string =  //takes in input and checks if it exsists (see isoccupied for position che)
    // This draws the board for us, Ernest's personal preference is clearing the console so it's more like a "game"
    System.Console.Clear()
    drawBoard board
    
    // Work out what to tell the player
    let msg = sprintf "%A's  turn: type the Y value of the cell that you want to %s. 
             Only accepts letters A ; B ; C ; D; E ; F ; G" (getIcon (swapPlayerToInt player)) sentence // sentence = "play into" for place state as appose to "move from " alterative for move state
    consColorWrite msg
    let i = (System.Convert.ToString (System.Console.ReadKey true).KeyChar).ToUpper() // Making it upper means we can use upper or lower case for input typing
    match i with 
    | "A" | "B" | "C" | "D" | "E" | "F" | "G" ->
        let d = string i // Saves Y value into d
        printfn "Type the X coordinate of the position that you want to %s.
                 EG '3' (Between 1 - 7) " sentence // Sentence = "play into" for place state as apposed to "move from " alterative for move state
        let n = (System.Console.ReadKey true).KeyChar
        match n with
        | '1' | '2' | '3' | '4' | '5' | '6' | '7' ->
            match  mrbaToFlat (d + string(n))    with
            | [9;9]-> 
                // This is the error state
                // Check move
                interaction player board sentence expect    // Potentially change player options
            | _ -> match inAct board (mrbaToFlat (d + string(n))) expect   with
                   |true -> (d + string(n) )                //final return case -> Saves value into n and smushes input into string EG "A1'
                   |_ ->
                        writeError "Sorry, you couldnt choose that point. Please try again!"
                        System.Threading.Thread.Sleep(2000)
                        interaction player board sentence expect
        | _ -> interaction player board sentence expect //invalid input, try again silly hooman      
    | _ -> interaction player board sentence expect     //invalid input, try again silly hooman 

/// <summary>
/// Returns the opposit player
/// </summary>
/// <param name="player"></param>
let otherplayer (player : Player) : Player =
    match player with 
    |Red -> Blue
    |_ -> Red

/// <summary>
/// When you shoot a cow, this is the function called
/// </summary>
/// <param name="point">This is the point</param>
/// <param name="victim">The victim you are shooting</param>
/// <param name="board">The board we are using</param>
/// <param name="player">The current player</param>
let shoot (point : int) (victim: Player) (board: int list) (player : Player): int list = //Still need to make sure the victim cow is not in a mill
    System.Console.Clear()
    drawBoard board
    System.Console.WriteLine("MIll Formed. Here I go killin' again")
    System.Threading.Thread.Sleep(2000)

    let point = interaction player board "to shoot" (otherplayer player)
    updateboard removecow victim ((mrbaToFlat point).[0] + (mrbaToFlat point).[1]) board

/// <summary>
/// Placing a cow
/// </summary>
/// <param name="mills">The built up mills we have in DataSets</param>
/// <param name="player">The current player</param>
/// <param name="cowsleft">The number of cows we have left in the game to place (? Ernest unsure)</param>
/// <param name="board">The board we are using</param>
/// <param name="ismill">The function we use to work out if there is a mill (? Ernest unsure : we can move this)</param>
let rec place (player : Player) (cowsleft : int) (board : int list) : int list = //SKELETON //cowsleft should be 24 when first called
    match cowsleft with
    |0 -> board
    |_ ->                                                   
        let spot = interaction player board "play into" Neither // accepted input
        let spott = match (mrbaToFlat spot) with            // Testing if use input is valid
                    |[9;9] -> [9;9]                         // TEMP. fail case goes here (? Ernest unsure)
                    |_ -> mrbaToFlat spot

        let board = updateboard (insertcow) player (spott.[0] + spott.[1]) (*<- this is x+y. see ReadMe*) board

        let boarda =                                        //checks if cow is in a mill, shoots if it is
            match (ismill board (spott) (swapPlayerToInt player)) with
            |true -> shoot (spott.[0] + spott.[1]) (otherplayer player) board player
            |false -> board 
            
        match player with 
        | Red -> place Blue (cowsleft-1) boarda      //Go to Next Move
        |_ -> place Red (cowsleft-1) boarda
        
/// <summary>
/// In progress version allows the 24 placement moves (? Ernest unsure)
/// </summary>
/// <param name="movesleft">The number of placements left (? Ernest unsure : we might change this slightly)</param>
/// <param name="mills">The list of mills we can search through</param>
/// <param name="player">The current player</param>
/// <param name="board">The board we are using</param>
/// <param name="ismill">The function we use to work out if there is a mill (? Ernest unsure : we can move this)</param>
let rec move (movesleft : int) (player : Player) (board : int list) (ismill : Mill list -> int list -> int list -> int -> bool) : int list =        //SKELETON 
    let froms = interaction player board "move from" player // take in user input
    let movetos = interaction player board "move to" Neither     // take in user input
    let (from : int list) = mrbaToFlat froms
    let (moveto : int list) = mrbaToFlat movetos 

    let oneaway (from: int list) (moveto : int list) : bool = //checks if move position is 1 away
        let a = (moveto.[0] = (from.[0] + 1)) || (moveto.[0] = (from.[0] - 1))
        let b = (moveto.[0] = (from.[0] + 7)) || (moveto.[0] = (from.[0] - 7))
        let c = (moveto.[1] = (from.[1] + 8)) || (moveto.[1] = (from.[1] - 8))
        match  a, b, c with
        | true, false, false
        | false, true, false
        | false, false, true ->
            match board.[moveto.[0] + moveto.[1]]=0 with
            | true -> true
            | _ -> false
        | _ -> false

    let allgood = match oneaway from moveto with
                  |true ->
                        let b = updateboard removecow player (from.[0] + from.[1]) board
                        updateboard insertcow player (moveto.[0]+moveto.[1]) b
                  //board is updated to remove cow. this result is passed into the next update which adds the cow to its new position.           
                  |_ -> move movesleft player board ismill //move movesleft mills 

    let boarda = //checks if cow is in a mill, shoots if it is
            match (ismill mills allgood (moveto) (swapPlayerToInt player)) with
            |true -> shoot (moveto.[0] + moveto.[1]) (otherplayer player) allgood player
            |false -> allgood 

    printf "%A \n" boarda
    match movesleft with
    |0 -> boarda
    |_ -> match player with 
          | Red -> move (movesleft-1) Blue (boarda) ismill 
          |_ -> move (movesleft-1) Red (boarda) ismill

//---------------------------------------------------------------------------------
// WE START HERE
//---------------------------------------------------------------------------------
let (placedboard : int list) = [0]
[<EntryPoint>]
let main argv = 
    move 20 Blue (place Blue 24 flatboard)
    0