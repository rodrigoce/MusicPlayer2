using Id3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer2
{
    public class PlayerControlsFileStorage
    {
        private Dictionary<string, string> _controlsValues = null;
        public PlayerControlsFileStorage()
        {
            _controlsValues = new Dictionary<string, string>();
            if (File.Exists(Path.GetTempPath() + Constants.CONTROLS_FILENAME))
            {
                File.ReadAllLines(Path.GetTempPath() + Constants.CONTROLS_FILENAME)
                    .ToList()
                    .ForEach(line =>
                    {
                        var parts = line.Split(new char[] { Constants.RECORD_SEPARATOR }, 2);
                        if (parts.Length == 2)
                            _controlsValues.Add(parts[0], parts[1]);
                    });
            }
        }
        public void SaveValue(string control, string value)
        {
            _controlsValues[control] = value;
            string lines = _controlsValues
                .Select(kvp => $"{kvp.Key}{Constants.RECORD_SEPARATOR}{kvp.Value}")
                .Aggregate((current, next) => current + Environment.NewLine + next);
            File.WriteAllText(Path.GetTempPath() + Constants.CONTROLS_FILENAME, lines);
        }

        public double? LoadDouble(string control)
        {
            if (_controlsValues.ContainsKey(control))
            {
                if (double.TryParse(_controlsValues[control], out double result))
                    return result;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
    }
}
