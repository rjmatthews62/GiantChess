using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace RoughChess
{
    public class Position
    {
        public int x, y;
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Position(int index)
        {
            this.x = index % 8;
            this.y = index / 8;
        }

        public int AsIndex()
        {
            return (this.y * 8) + (this.x);
        }

        public bool IsValid()
        {
            return (this.x >= 0 && this.x <= 7 && this.y >= 0 && this.y <= 7);
        }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        public string AsMove()
        {
            return $"{x},{y}";
        }
    }

    public class Move
    {
        public int x, y;
        public Move(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class AiResult
    {
        public Position from;
        public Position dest;
        public int score;
        public bool success = false;
        public DateTime started;

        public AiResult()
        {
            score = 0;
            dest = null;
            from = null;
            success = false;
        }
        public AiResult(Position from, Position dest, int score)
        {
            this.from = from;
            this.dest = dest;
            this.score = score;
        }

        public AiResult(int from, int dest, int score)
        {
            this.from = new Position(from);
            this.dest = new Position(dest);
            this.score = score;
        }
    }

    public class AiState
    {
        internal List<Move> possibles = new List<Move>();
        public int bestScore = 0;
        public int player;
        public string board;
        public int lookAhead;
        public int boardLocation = 0;
        public int currentScore = 0;
        public AiResult result = null;
        internal List<int> moves = null;

        internal void AddMove(int score, Move m)
        {

            if (possibles.Count == 0 || score > bestScore)
            {
                possibles.Clear();
                possibles.Add(m);
                bestScore = score;
            }
            else if (score == bestScore) possibles.Add(m);
        }

    }

    class ChessLogic
    {
        // list of pieces and associated imaged. Using uppercase for white, lowercase for black.
        public readonly Dictionary<string, string> pieces = new Dictionary<string, string>()
        {
            {"k", "KingB" },
            {"q", "QueenB"},
            {"b", "BishopB"},
            {"n", "KnightB"},
            {"r", "RookB"},
            {"p", "PawnB"},
            {"K", "KingW"},
            {"Q", "QueenW"},
            {"B", "BishopW"},
            {"N", "KnightW"},
            {"R", "RookW"},
            {"P", "PawnW"},
            {" ", "transparent"},
            {"*", "cloud"}
        };
        readonly string[] sidepieces = { "KQBNRP", "kqbnrp" };  // Array containing allowed pieces for each side to move 

        public readonly string basicsetup = "rnbqkbnr" +  // Standard layout.
                "pppppppp" +
                "        " +
                "        " +
                "        " +
                "        " +
                "PPPPPPPP" +
                "RNBQKBNR";

        public string boardPos; // boardPos contains the current board layout. Using a 64 char string.
        string moveFrom = "";  // Current selected 'From'
        string moveTo = "";   // Current selected 'to'
        public int player = 0;   // Current player. 0=white, 1=black.
        //bool moveMade = false;
        public string movemessage = ""; // Specific details on why a move would be illegal.
        public readonly List<string> movelist = new List<string>();
        readonly System.Random random = new System.Random();

        public bool threadReady = false;
        public AiResult threadResult;

        private List<AiResult> enumLevels = new List<AiResult>();

        public ChessLogic()
        {
            boardPos = basicsetup;
        }
        /// <summary>
        ///  Change which player is currently active.
        /// </summary>
        /// <returns></returns>
        public int Switchplayer()
        {
            player = (player + 1) % 2;
            return player;
        }

        // Translate an index to x and y positons.
        public static Position IndexXY(int n)
        {
            int x = n % 8;
            int y = n / 8;
            return new Position(x, y);
        }

        static bool IsLower(string s)
        {
            return s.ToLower().Equals(s);
        }

        // Checks if a destination square contains an enemy piece
        bool IsEnemy(string piece, Position pos, string aBoard)
        {
            if (!pos.IsValid()) return false;
            var destpiece = aBoard.Substring(pos.AsIndex(), 1);
            if (destpiece == " ") return false;
            return IsLower(piece) != IsLower(destpiece);
        }

        // Checks if a destination square contains a friendly piece
        bool IsFriendly(string piece, Position pos, string aBoard)
        {
            if (!pos.IsValid()) return false;
            var destpiece = aBoard.Substring(pos.AsIndex(), 1);
            if (destpiece == " ") return false;
            return IsLower(piece) == IsLower(destpiece);
        }

        bool IsFriendly(int aPlayer, string board, Position pos)
        {
            if (!pos.IsValid()) return false;
            var destpiece = board.Substring(pos.AsIndex(), 1);
            if (destpiece == " ") return false;
            return aPlayer == 0 ? !IsLower(destpiece) : IsLower(destpiece);
        }

        // Add a move to a result array, checking that it is inside the board
        static void AddResult(List<int> result, Position pos)
        {
            if (!pos.IsValid()) return;
            var n = pos.AsIndex();
            if (!result.Contains<int>(n)) result.Add(n);
        }

        // Add moves until we either go off the page or run into another piece.
        bool AddUntilBlocked(List<int> result, Position pos, string piece, string aBoard)
        {
            if (!pos.IsValid()) return false;
            AddResult(result, pos);
            if (IsEnemy(piece, pos, aBoard) || IsFriendly(piece, pos, aBoard)) return false;
            return true;
        }

        public void AiMoveThreaded(int aPlayer, string aBoard, int lookAhead)
        {
            threadReady = false;
            Thread thread = new Thread(() =>
            {
                threadResult = AiMove(aPlayer, aBoard, lookAhead);
                threadReady = true;

            });
            thread.Start();
        }


        public AiResult AiMove(int aPlayer, string aBoard, int lookAhead)
        {
            List<Move> possibles = new List<Move>();
            int bestScore = 0;

            for (int i = 0; i < 64; i++)
            {
                Position fpos = IndexXY(i);
                if (IsFriendly(aPlayer, aBoard, fpos))
                {
                    String piece = aBoard.Substring(i, 1);
                    var moves = GenerateMoves(piece, i, aPlayer, false, aBoard);
                    foreach (int n in moves)
                    {
                        string brd = PerformMove(aPlayer, i, n, aBoard);
                        if (String.IsNullOrEmpty(brd)) continue; // Illegal move, possibly check.
                        int score = ScorePosition(aPlayer, brd);
                        if (lookAhead > 0)
                        {
                            int otherPlayer = (player + 1) % 2;
                            AiResult r = AiMove(otherPlayer, brd, lookAhead - 1);
                            if (r.success)
                            {
                                score -= r.score;
                            }
                            else
                            {
                                if (CheckCheck(otherPlayer, brd)) score += 999; // Checkmate.
                                else score -= 10; // Draw is less desirable...
                            }
                        }
                        if (possibles.Count == 0 || score > bestScore)
                        {
                            possibles.Clear();
                            possibles.Add(new Move(i, n));
                            bestScore = score;
                        }
                        else if (score == bestScore) possibles.Add(new Move(i, n));
                    }
                }
            }
            if (possibles.Count == 0) return new AiResult(0, 0, 0); // No moves. Check or Stalemate.
            int pick = random.Next(possibles.Count);
            Move m = possibles[pick];
            AiResult result = new AiResult(m.x, m.y, bestScore);
            result.success = true;
            return result;
        }

        public AiState AiMoveState(int aPlayer, string aBoard, int lookAhead)
        {
            AiState result = new AiState();
            result.player = aPlayer;
            result.board = aBoard;
            result.lookAhead = lookAhead;
            AiMoveNext(ref result);
            return result;
        }

        public bool AiMoveNext(ref AiState state)
        {
            while (state.boardLocation < 64)
            {
                int i = state.boardLocation;
                Position fpos = IndexXY(i);
                if (IsFriendly(state.player, state.board, fpos))
                {
                    String piece = state.board.Substring(i, 1);
                    if (state.moves == null) state.moves = GenerateMoves(piece, i, state.player, false, state.board);
                    int counter=1;
                    while (state.moves.Count > 0)
                    {
                        int n = state.moves[0];
                        state.moves.RemoveAt(0);
                        string brd = PerformMove(state.player, i, n, state.board);
                        if (String.IsNullOrEmpty(brd)) continue; // Illegal move, possibly check.
                        int score = ScorePosition(state.player, brd);
                        if (state.lookAhead > 0)
                        {
                            int otherPlayer = (player + 1) % 2;
                            AiResult r = AiMove(otherPlayer, brd, state.lookAhead - 1);
                            if (r.success)
                            {
                                score -= r.score;
                            }
                            else
                            {
                                if (CheckCheck(otherPlayer, brd)) score += 999; // Checkmate.
                                else score -= 10; // Draw is less desirable...
                            }
                        }
                        state.AddMove(score, new Move(i, n));
                        if (--counter<=0) return false;
                    }
                    state.moves=null;
                }
                state.boardLocation += 1;
            }
            if (state.possibles.Count == 0)
            {
                state.result = new AiResult(0, 0, 0); // No moves. Check or Stalemate.
            }
            else
            {
                int pick = random.Next(state.possibles.Count);
                Move m = state.possibles[pick];
                AiResult result = new AiResult(m.x, m.y, state.bestScore);
                result.success = true;
                state.result = result;
            }
            return true;
        }

        public bool isCheck()
        {
            return CheckCheck(player, boardPos);
        }

        private string PerformMove(int aPlayer, int from, int dest, string aBoard)
        {
            var c = aBoard.Substring(from, 1);
            var destpiece = aBoard.Substring(dest, 1);
            if (!LegalMove(aPlayer, aBoard, from, dest))
            {
                return null;
            }
            aBoard = aBoard.Substring(0, from) + " " + aBoard.Substring(from + 1);
            aBoard = aBoard.Substring(0, dest) + c + aBoard.Substring(dest + 1);
            if (CheckCheck(aPlayer, aBoard))
            {
                return null;
            }
            aBoard = CheckPromotion(aPlayer, c, IndexXY(dest), aBoard);
            return aBoard;
        }

        private int ScorePosition(int aPlayer, string aBoard)
        {
            int result = 0;
            for (int i = 0; i < 64; i++)
            {
                string piece = aBoard.Substring(i, 1);
                if (piece == " ") continue;
                int score = CalcScore(piece);
                if (IsFriendly(aPlayer, aBoard, IndexXY(i))) result += score; else result -= score;
            }
            return result;
        }

        private int CalcScore(string piece)
        {
            switch (piece.ToLower())
            {
                case "p": return 1;
                case "n":
                case "b": return 3;
                case "r": return 5;
                case "q": return 9;
                case "k": return 99; // Need to think this through.
                default: return 0;
            }
        }

        // Generate legal piece moves. The 'vision' parameter will be used to generate 'fog of war'
        List<int> GenerateMoves(string piece, int posfrom, int aplayer, bool vision, string aBoard)
        {
            List<int> result = new List<int>();
            var startpos = IndexXY(posfrom);
            Position newpos;
            int x;
            int y;
            switch (piece.ToLower())
            {
                case "p": // Pawn. Paradoxically, one of the more complex things to work out.
                    var dir = aplayer == 0 ? -1 : 1; // Are we going up or down the board?
                    newpos = new Position(startpos.x, startpos.y + dir);
                    if (vision || !IsEnemy(piece, newpos, aBoard)) AddResult(result, newpos);
                    newpos = new Position(startpos.x - 1, startpos.y + dir);
                    if (vision || IsEnemy(piece, newpos, aBoard)) AddResult(result, newpos);
                    newpos = new Position(startpos.x + 1, startpos.y + dir);
                    if (vision || IsEnemy(piece, newpos, aBoard)) AddResult(result, newpos);
                    if ((aplayer == 0 && startpos.y == 6) || (aplayer == 1 && startpos.y == 1))
                    { // Initial double move
                        newpos = new Position(startpos.x, startpos.y + (dir * 2));
                        if (vision || !IsEnemy(piece, newpos, aBoard)) AddResult(result, newpos);
                    }
                    break;
                case "b": // Bishop
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x + i, startpos.y + i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x - i, startpos.y + i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x - i, startpos.y - i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x + i, startpos.y - i), piece, aBoard)) break;
                    break;
                case "r": // rook
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x + i, startpos.y), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x - i, startpos.y), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x, startpos.y + i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x, startpos.y - i), piece, aBoard)) break;
                    break;
                case "q": // queen
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x + i, startpos.y + i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x - i, startpos.y + i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x - i, startpos.y - i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x + i, startpos.y - i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x + i, startpos.y), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x - i, startpos.y), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x, startpos.y + i), piece, aBoard)) break;
                    for (var i = 1; i < 8; i++) if (!AddUntilBlocked(result, new Position(startpos.x, startpos.y - i), piece, aBoard)) break;
                    break;
                case "k": // king
                    for (y = -1; y <= 1; y++)
                    {
                        for (x = -1; x <= 1; x++)
                        {
                            if (x == 0 && y == 0) continue;
                            AddResult(result, new Position(startpos.x + x, startpos.y + y));
                        }
                    }
                    break;
                case "n": // knight
                    AddResult(result, new Position(startpos.x - 2, startpos.y - 1));
                    AddResult(result, new Position(startpos.x - 1, startpos.y - 2));
                    AddResult(result, new Position(startpos.x + 1, startpos.y - 2));
                    AddResult(result, new Position(startpos.x + 2, startpos.y - 1));
                    AddResult(result, new Position(startpos.x + 2, startpos.y + 1));
                    AddResult(result, new Position(startpos.x + 1, startpos.y + 2));
                    AddResult(result, new Position(startpos.x - 1, startpos.y + 2));
                    AddResult(result, new Position(startpos.x - 2, startpos.y + 1));
                    break;
            }
            return result;
        }

        static string SetBoardPiece(string aboard, int index, string newpiece)
        {
            return aboard.Substring(0, index) + newpiece + aboard.Substring(index + 1);
        }

        void CheckPromotion(string piece, Position pos)
        {
            boardPos = CheckPromotion(player, piece, pos, boardPos);
        }

        string CheckPromotion(int aPlayer, string piece, Position pos, string aBoard)
        {
            if (piece.ToLower() != "p") return aBoard;
            if (aPlayer == 0 && pos.y == 0) aBoard = SetBoardPiece(aBoard, pos.AsIndex(), "Q");
            else if (aPlayer == 1 && pos.y == 7) aBoard = SetBoardPiece(aBoard, pos.AsIndex(), "q");
            return aBoard;
        }

        // Helper function to set an error message and return false.
        bool BadMove(string msg)
        {
            movemessage = msg;
            return false;
        }

        // Check that a move is legal ... currently a work in progress,
        bool LegalMove(int aPlayer, string aBoard, int posfrom, int posto)
        {
            movemessage = "";
            //            if (movemade) return badmove("Not your turn. Hit Hide to let other player move.");
            // First, check is current players piece
            var piece = aBoard.Substring(posfrom, 1);
            var allowedpieces = sidepieces[aPlayer];
            if (!allowedpieces.Contains(piece)) return BadMove("Not your piece");
            // See if this move is allowed for this piece.
            var moves = GenerateMoves(piece, posfrom, aPlayer, false, aBoard);
            if (!moves.Contains<int>(posto)) return BadMove("Not a valid move");
            if (IsFriendly(piece, IndexXY(posto), aBoard)) return BadMove("Destination is Friendly");
            return true;
        }

        public static int ParseMove(string move)
        {
            int x, y;
            string[] m = move.Split(',');
            if (m.Length < 2) return -1;
            if (!Int32.TryParse(m[0], out x)) return -1;
            if (!Int32.TryParse(m[1], out y)) return -1;
            if (x < 0 || x > 7 || y < 0 || y > 7) return -1;
            return (y * 8) + x;
        }

        // Execute a requested move.
        bool DoMove()
        {
            int f = ParseMove(moveFrom);
            int t = ParseMove(moveTo); ;
            if (f < 0) return BadMove("Invalid From");
            if (t < 0) return BadMove("Invalid To");
            var c = boardPos.Substring(f, 1);
            var oldpos = boardPos;
            var destpiece = boardPos.Substring(t, 1);
            if (!LegalMove(player, boardPos, f, t))
            {
                ClearSel();
                return false;
            }
            boardPos = boardPos.Substring(0, f) + " " + boardPos.Substring(f + 1);
            boardPos = boardPos.Substring(0, t) + c + boardPos.Substring(t + 1);
            if (CheckCheck(player, boardPos))
            {
                boardPos = oldpos;
                ClearSel();
                return false;

            }
            var oldp = IndexXY(f);
            var newp = IndexXY(t);
            var move = (player == 0 ? "W" : "B") + " " + c + oldp + "-" + newp;
            if (destpiece != " ") move += " takes " + destpiece;
            movelist.Insert(0, move);
            CheckPromotion(c, newp);
            ClearSel();
            Switchplayer();
            return true;
        }



        public bool ExecuteMove(string from, string dest)
        {
            moveFrom = from;
            moveTo = dest;
            return DoMove();
        }

        // Check if King is in check
        private bool CheckCheck(int player, string aBoard)
        {
            var p = (player == 0) ? "K" : "k";
            var enemy = (player + 1) % 2;
            var n = aBoard.IndexOf(p); // Find where the king is.
            if (n < 0) return false; // Something has gone horribly wrong...
            for (var i = 0; i < 64; i++)
            {
                var piece = aBoard.Substring(i, 1);
                var pos = IndexXY(i);
                if (IsEnemy(p, pos, aBoard))
                {
                    var moves = GenerateMoves(piece, i, enemy, false, aBoard);
                    if (moves.Contains<int>(n)) return true;
                }
            }
            return false;
        }

        private void ClearSel()
        {
            moveFrom = "";
            moveTo = "";
        }

        // Set the board back to the basic position, set player to 'White'
        public void ResetBoard()
        {
            boardPos = basicsetup;
            player = 0;
            movelist.Clear();
        }

        internal string Display()
        {
            StringBuilder b = new StringBuilder();
            for (int y = 0; y < 8; y++)
            {
                b.AppendLine(boardPos.Substring(y * 8, 8));
            }
            return b.ToString();
        }
    }

}
