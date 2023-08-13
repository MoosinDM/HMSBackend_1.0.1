using System;
using System.IO;

namespace HMSBackend.Models
{
    public class UniqueIdGenerator
    {
        private readonly object _lock = new object();
        private int _sequenceNumber;
        private DateTime _lastGeneratedDate;

        public UniqueIdGenerator()
        {
            InitializeState();
        }

        private void InitializeState()
        {
            string stateFilePath = "sequence_state.txt";

            if (File.Exists(stateFilePath))
            {
                string[] lines = File.ReadAllLines(stateFilePath);
                if (lines.Length == 2 && int.TryParse(lines[0], out int sequence) && DateTime.TryParse(lines[1], out DateTime date))
                {
                    _sequenceNumber = sequence;
                    _lastGeneratedDate = date;
                }
            }
            else
            {
                _sequenceNumber = 0;
                _lastGeneratedDate = DateTime.MinValue;
            }
        }

        public string GenerateUniqueId(string prefix)
        {
            lock (_lock)
            {
                DateTime currentDate = DateTime.UtcNow;
                if (currentDate.Date != _lastGeneratedDate.Date)
                {
                    _lastGeneratedDate = currentDate;
                    _sequenceNumber = 1;
                }
                else
                {
                    _sequenceNumber++;
                }

                string formattedDate = currentDate.ToString("yyMMdd");
                string uniqueId = $"{prefix}{formattedDate}{_sequenceNumber:D3}";

                UpdateState();

                return uniqueId;
            }
        }

        private void UpdateState()
        {
            string stateFilePath = "sequence_state.txt";

            using (StreamWriter writer = new StreamWriter(stateFilePath, false))
            {
                writer.WriteLine(_sequenceNumber);
                writer.WriteLine(_lastGeneratedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }
}

