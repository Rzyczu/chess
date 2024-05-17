﻿using chess.Enums;

namespace chess.Pieces
{
    public class Rook : Piece
    {
        public bool IsMoved { get; set; }

        public Rook(Coordinates coordinates, Player player) : base(coordinates, player, PieceType.Rock)
        {
            IsMoved = false;
        }

        public override bool IsValidMove(Coordinates start, Coordinates end, GameBoard board)
        {
            // Rook moves horizontally or vertically
            return start.X == end.X || start.Y == end.Y;
        }
    }
}