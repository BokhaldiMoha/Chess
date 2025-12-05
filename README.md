<p>This is a complete chess game. The project is structured into a dedicated logic library and a Blazor web application with the user interface to play the game.</p>
<p>The code is split into two main projects to keep the core rules separate from the presentation: Chess_Logic which is a C# class library containing all the rules, move calculation, and game state management. And Chess_Blazor, the web application that consumes the Chess_Logic library to provide a playable front-end.</p>
<p>Complete Rule Set: Full support for standard moves, captures, and advanced game mechanics.</p>
<p>Accurate Movement: Rigorous calculation and validation of legal moves for all pieces.</p>
<p>Endgame Handling: Correct detection of Checkmate,and all possible Draw conditions.</p>
<p>Special Moves: Fully implemented support for Castling and En Passant.</p>

<p>Current limitation is pawn promotion, a pawn can only be promoted to a Queen.</p>
<p>Also there is no support for time limit for players.</p>
