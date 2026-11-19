using RelicAndCoin.Core.Stats;
using System.Collections.Generic;

namespace RelicAndCoin.Combat.Runes
{
    /// <summary>
    /// Pure C# — quản lý các ô khảm Rune trên trang bị / vũ khí.
    /// Gắn Rune vào ô trống, tháo Rune, áp dụng / gỡ bỏ toàn bộ StatModifier của Rune
    /// vào CharacterStatsModel thông qua source-based removal.
    /// Không phụ thuộc UnityEngine.
    /// </summary>
    public sealed class RuneSocketService
    {
        /// <summary>Thông tin một ô khảm: đang có Rune hay không, Rune đang gắn.</summary>
        public sealed class Socket
        {
            public bool   IsOccupied => _rune != null;
            public RuneDataSO Rune   => _rune;

            private RuneDataSO _rune;

            internal Socket() { }

            internal void SetRune(RuneDataSO rune) => _rune = rune;
            internal void Clear() => _rune = null;
        }

        private readonly List<Socket> _sockets = new();

        /// <summary>Tạo <paramref name="count"/> ô khảm trống.</summary>
        public RuneSocketService(int count)
        {
            for (int i = 0; i < count; i++)
                _sockets.Add(new Socket());
        }

        public int SocketCount => _sockets.Count;
        public int OccupiedCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _sockets.Count; i++)
                    if (_sockets[i].IsOccupied) n++;
                return n;
            }
        }

        /// <summary>Lấy ô khảm theo chỉ số (ném nếu ngoài phạm vi).</summary>
        public Socket GetSocket(int index) => _sockets[index];

        /// <summary>
        /// Gắn Rune vào ô trống chỉ số <paramref name="index"/>, áp dụng StatModifier vào model.
        /// Trả về true nếu thành công, false nếu ô đã có Rune hoặc chỉ số không hợp lệ / Rune null.
        /// </summary>
        public bool InsertRune(int index, RuneDataSO rune, CharacterStatsModel model)
        {
            if (index < 0 || index >= _sockets.Count) return false;
            if (rune == null) return false;
            var socket = _sockets[index];
            if (socket.IsOccupied) return false;

            socket.SetRune(rune);
            ApplyRuneModifiers(rune, model, add: true);
            return true;
        }

        /// <summary>
        /// Tháo Rune khỏi ô chỉ số <paramref name="index"/>, gỡ StatModifier khỏi model.
        /// Trả về RuneDataSO đã tháo (hoặc null nếu không thành công).
        /// </summary>
        public RuneDataSO RemoveRune(int index, CharacterStatsModel model)
        {
            if (index < 0 || index >= _sockets.Count) return null;
            var socket = _sockets[index];
            if (!socket.IsOccupied) return null;

            var rune = socket.Rune;
            socket.Clear();
            ApplyRuneModifiers(rune, model, add: false);
            return rune;
        }

        /// <summary>Tháo tất cả Rune, gỡ toàn bộ modifier của chúng khỏi model.</summary>
        public void ClearAllSockets(CharacterStatsModel model)
        {
            for (int i = _sockets.Count - 1; i >= 0; i--)
            {
                var socket = _sockets[i];
                if (!socket.IsOccupied) continue;
                var rune = socket.Rune;
                socket.Clear();
                ApplyRuneModifiers(rune, model, add: false);
            }
        }

        private static void ApplyRuneModifiers(RuneDataSO rune, CharacterStatsModel model, bool add)
        {
            if (rune == null || model == null) return;

            // Mỗi RuneDataSO instance dùng làm Source để source-based removal gỡ sạch
            // chính modifier của rune đó mà không ảnh hưởng modifier khác cùng stat.
            object source = rune;

            foreach (StatModifier mod in rune.StatModifiers)
            {
                if (add) model.AddModifier(new StatModifier(mod.Stat, mod.Type, mod.Value, source));
                else     model.RemoveModifiersBySource(source);
            }
        }
    }
}
