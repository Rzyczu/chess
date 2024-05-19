﻿using chess;
using chess.Enums;
using chess.Pieces;

public class Game
{
    private GameBoard board;
    private Player player1;
    private Player player2;
    private List<Turn> turnsHistory;
    private Turn currentTurn;

    public Game()
    {
        player1 = new Player(ColorType.White);
        player2 = new Player(ColorType.Black);
        board = new GameBoard(8, 8);
        SetInitialPieces();
        turnsHistory = new List<Turn>();
        currentTurn = new Turn(1, player1);
    }

    private void SetInitialPieces()
    {
        PlacePiecesForPlayer(player1, 0);
        PlacePiecesForPlayer(player2, 7);

        for (int col = 0; col < board.Width; col++)
        {
            board.AddPiece(new Pawn(new Coordinates(1, col), player1), new Coordinates(1, col));
            board.AddPiece(new Pawn(new Coordinates(board.Height - 2, col), player2), new Coordinates(6, col));
        }
    }

    private void PlacePiecesForPlayer(Player player, int row)
    {
        // Place Rooks
        board.AddPiece(new Rook(new Coordinates(row, 0), player), new Coordinates(row, 0));
        board.AddPiece(new Rook(new Coordinates(row, 7), player), new Coordinates(row, 7));
        // Place Knights
        board.AddPiece(new Knight(new Coordinates(row, 1), player), new Coordinates(row, 1));
        board.AddPiece(new Knight(new Coordinates(row, 6), player), new Coordinates(row, 6));
        // Place Bishops
        board.AddPiece(new Bishop(new Coordinates(row, 2), player), new Coordinates(row, 2));
        board.AddPiece(new Bishop(new Coordinates(row, 5), player), new Coordinates(row, 5));
        // Place Queen
        board.AddPiece(new Queen(new Coordinates(row, 3), player), new Coordinates(row, 3));
        // Place King
        board.AddPiece(new King(new Coordinates(row, 4), player), new Coordinates(row, 4));
    }

    public void StartGame()
    {
        Console.WriteLine("Welcome! Starting new game of chess...\n\n\n");
        Console.WriteLine($"Turn: {currentTurn.Number}\n");
        PrintBoard();
        PlayGame();
    }

    private void PlayGame()
    {
        while (!IsGameOver())
        {
            ExecuteTurn();
            Console.WriteLine($"\n------------------------------------------\n");
            Console.WriteLine($"Turn: {currentTurn.Number}\n");
            PrintBoard();
            if (IsGameOver())
            {
                Console.WriteLine("Game over!");
                PrintResult();
            }
        }
    }

    private void ExecuteTurn()
    {
        Console.WriteLine($"\nPlayer {currentTurn.Player.Color}'s turn\n");
        MakeMove();
        UpdateTurn();
    }

    private void MakeMove()
    {
        Console.WriteLine("Enter your move (e.g., 'e2 e4'):");
        string moveInput = Console.ReadLine();
        string[] moveParts = moveInput.Split(' ');

        if (moveParts.Length != 2 || !IsValidInput(moveParts[0]) || !IsValidInput(moveParts[1]))
        {
            Console.WriteLine(ErrorMessages.MoveFormatInputError);
            MakeMove();
            return;
        }

        string startPosition = moveParts[0];
        string endPosition = moveParts[1];

        Coordinates start = AlgebraicToCoordinates(startPosition);
        Coordinates end = AlgebraicToCoordinates(endPosition);

        if (!board.IsWithinBounds(start) || !board.IsWithinBounds(end))
        {
            Console.WriteLine(ErrorMessages.WithinBoundError);
            MakeMove(); // Retry move
            return;
        }

        Piece pieceAtStart = board.GetPieceAt(start);

        if (pieceAtStart == null)
        {
            Console.WriteLine(ErrorMessages.PieceStartError);
            MakeMove(); // Retry move
            return;
        }

        Move move = new Move(start, end, pieceAtStart, null);

        if (!IsValidMove(move))
        {
            Console.WriteLine(ErrorMessages.InvalidMoveError);
            MakeMove(); // Retry move
            return;
        }

        currentTurn.Move = move;

        // Check for castling
        if (pieceAtStart is King && Math.Abs(start.Y - end.Y) == 2)
        {
            if (IsValidCastling(move))
            {
                HandleCastling(move);
            }
            else
            {
                MakeMove(); // Retry move
                return;
            }
        }
        ExecuteMove(move);
    }

    private bool IsValidInput(string position)
    {
        return position.Length == 2 &&
               position[0] >= 'a' && position[0] <= 'h' &&
               position[1] >= '1' && position[1] <= '8';
    }

    private bool IsValidCastling(Move move)
    {

        Coordinates start = move.StartPosition;
        Coordinates end = move.EndPosition;
        Piece king = move.PiecePlayed;

        int deltaY = Math.Sign(end.Y - start.Y);
        int rookY = end.Y + (deltaY == 1 ? 1 : (deltaY == -1 ? -2 : 0));

        Piece rook = board.GetPieceAt(new Coordinates(start.X, rookY));

        // Check if both the king and the rook haven't moved
        if (!(king is King kingPiece && !kingPiece.IsMoved) || !(rook is Rook rookPiece && !rookPiece.IsMoved))
        {
            Console.WriteLine(ErrorMessages.CatlingPieceMovedError);
            return false;
        }

        // Check if there are any pieces between the king and the rook
        int rookEndY = rookY - deltaY;
        for (int col = Math.Min(start.Y, rookEndY) + 1; col < Math.Max(start.Y, rookEndY); col++)
        {
            if (board.GetPieceAt(new Coordinates(start.X, col)) != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ErrorMessages.CastlingPacthError);
                Console.ResetColor();
                return false;
            }
        }

        return true;

    }
    private void HandleCastling(Move move)
    {

        Coordinates start = move.StartPosition;
        Coordinates end = move.EndPosition;
        Piece king = move.PiecePlayed;
        int deltaY = Math.Sign(end.Y - start.Y);
        int rookY = end.Y + (deltaY == 1 ? 1 : (deltaY == -1 ? -2 : 0));

        Piece rook = board.GetPieceAt(new Coordinates(start.X, rookY));
        int rookEndY = rookY - deltaY;

        // Move the rook
        Coordinates rookStart = new Coordinates(start.X, (end.Y == 6) ? 7 : 0);
        Coordinates rookEnd = new Coordinates(start.X, rookEndY);
        board.RemovePieceAt(rookStart);
        board.AddPiece(rook, rookEnd);
        rook.Coordinates = rookEnd;
        ((Rook)rook).IsMoved = true;

        // Move the king
        board.RemovePieceAt(start);
        board.AddPiece(king, end);
        king.Coordinates = end;
        ((King)king).IsMoved = true;

        turnsHistory.Add(currentTurn);
    }



    private void ExecuteMove(Move move)
    {
        Coordinates start = move.StartPosition;
        Coordinates end = move.EndPosition;
        Piece pieceAtStart = move.PiecePlayed;
        Piece pieceAtEnd = board.GetPieceAt(end);


        if (pieceAtEnd != null && pieceAtEnd.Player != pieceAtStart.Player)
        {
            board.RemovePieceAt(end);
            currentTurn.Player.Score++;
        }

        if (pieceAtStart.Type == PieceType.Pawn)
        {
            if (!((Pawn)pieceAtStart).IsMoved)
            {
                ((Pawn)pieceAtStart).IsMoved = true;
            }
        }
        if (pieceAtStart.Type == PieceType.King)
        {
            if (!((King)pieceAtStart).IsMoved)
            {
                ((King)pieceAtStart).IsMoved = true;
            }
        }
        if (pieceAtStart.Type == PieceType.Rook)
        {
            if (!((Rook)pieceAtStart).IsMoved)
            {
                ((Rook)pieceAtStart).IsMoved = true;
            }
        }

        board.RemovePieceAt(start);
        board.AddPiece(pieceAtStart, end);

        pieceAtStart.Coordinates = end;

        turnsHistory.Add(currentTurn);
    }



    private Coordinates AlgebraicToCoordinates(string algebraicNotation)
    {
        int row = algebraicNotation[1] - '1';
        int col = algebraicNotation[0] - 'a';
        return new Coordinates(row, col);
    }

    private void UpdateTurn()
    {
        currentTurn = new Turn(currentTurn.Number + 1, (currentTurn.Player == player1) ? player2 : player1);
    }

    private bool IsValidMove(Move move)
    {
        Piece piece = move.PiecePlayed;

        if (piece == null || piece.Player != currentTurn.Player)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ErrorMessages.EnemyPieceStartError(currentTurn.Player));
            Console.ResetColor();
            return false;
        }

        Coordinates start = move.StartPosition;
        Coordinates end = move.EndPosition;

        Piece pieceAtEnd = board.GetPieceAt(end);
        if (pieceAtEnd != null && pieceAtEnd.Player == currentTurn.Player)
        {
            return false;
        }

        if (!piece.IsValidMove(start, end, board))
        {
            return false;
        }

        if (!(piece is Knight) && !IsPathClear(start, end, board))
        {
            return false;
        }

        return true;
    }

    private bool IsPathClear(Coordinates start, Coordinates end, GameBoard board)
    {
        int deltaX = Math.Sign(end.X - start.X);
        int deltaY = Math.Sign(end.Y - start.Y);

        int currentX = start.X + deltaX;
        int currentY = start.Y + deltaY;

        while (currentX != end.X || currentY != end.Y)
        {
            if (board.GetPieceAt(new Coordinates(currentX, currentY)) != null)
            {
                return false; // Path is blocked by another piece
            }

            currentX += deltaX;
            currentY += deltaY;
        }

        return true; // Path is clear
    }

    private bool IsGameOver()
    {
        return false;
    }

    private void PrintBoard()
    {
        ConsoleColor lightSquareColor = ConsoleColor.DarkGreen;
        ConsoleColor darkSquareColor = ConsoleColor.DarkGray;
        ConsoleColor whiteColor = ConsoleColor.White;
        ConsoleColor blackColor = ConsoleColor.Black;

        Console.Write("   ");
        for (int col = 0; col < board.Width; col++)
        {
            Console.ForegroundColor = darkSquareColor;
            char columnLabel = (char)('A' + col);
            Console.Write(columnLabel.ToString().PadLeft(2).PadRight(3));
        }
        Console.WriteLine();

        for (int row = 0; row < board.Height; row++)
        {
            Console.ForegroundColor = darkSquareColor;
            Console.Write((row + 1).ToString().PadLeft(2) + " ");

            ConsoleColor squareColor = (row % 2 == 0) ? darkSquareColor : lightSquareColor;
            for (int col = 0; col < board.Width; col++)
            {
                Console.BackgroundColor = squareColor;

                Piece piece = board.GetPieceAt(new Coordinates(row, col));
                if (piece != null)
                {
                    ConsoleColor pieceColor = (piece.Player.Color == ColorType.White) ? whiteColor : blackColor;
                    Console.ForegroundColor = pieceColor;
                    Console.Write(piece.Type.ToString().Substring(0, 1).PadLeft(2).PadRight(3));
                }
                else
                {
                    Console.Write(" ".PadRight(3));
                }

                squareColor = (squareColor == darkSquareColor) ? lightSquareColor : darkSquareColor;
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private bool IsKingInCheck(Player player)
    {
        Piece king = board.GetPieceOfType(PieceType.King, player);
        if (king == null)
        {
            throw new InvalidOperationException("King not found on the board.");
        }

        // Get all opponent pieces
        List<Piece> opponentPieces = (player == player1) ? board.BlackPieces : board.WhitePieces;

        foreach (Piece piece in opponentPieces)
        {
            if (piece.IsValidMove(piece.Coordinates, king.Coordinates, board))
            {
                return true;
            }
        }
        return false;
    }


    private void PrintResult()
    {
        Console.WriteLine("Printing game result...");
    }



}
