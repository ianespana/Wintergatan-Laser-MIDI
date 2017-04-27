using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiSharp;
using System.IO;
using MidiSharp.Events.Voice.Note;
using MidiSharp.Events;

namespace WintergatanLaserMIDI
{
    class Program
    {
        static void Main(string[] args)
        {
            double toneDistance = 1;
            double beatDistance = 1;
            double n;
            List<string> lines = new List<string>();
            lines.Add("INSERT PUNCHHOLE");
            string path;
            double note;
            double ticks;
            double time;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: WintergatanLaserMIDI <filename.mid> [Tone Distance (mm)] [Beat Distance (mm)]");
                Console.WriteLine("\tfilename.mid = MIDI file for which to generate code");
                Console.WriteLine("\tTone Distance = Defines the distance between every note-pitch.");
                Console.WriteLine("\tBeat Distance = Defines how far away the beats are from each other.");
                Console.WriteLine();
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Error: file {0} not found", args[0]);
                return;
            }

            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\" + args[0] + ".scr";
            path = new Uri(path).LocalPath;

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (args.Length == 2 && !string.IsNullOrEmpty(args[1]))
            {
                if (double.TryParse(args[1], out n))
                {
                    toneDistance = n;
                }
                else
                {
                    Console.WriteLine("Tone Distance must be a number!");
                    return;
                }
            }

            if (args.Length == 3 && !string.IsNullOrEmpty(args[2]))
            {
                if (double.TryParse(args[2], out n))
                {
                    beatDistance = n;
                }
                else
                {
                    Console.WriteLine("Beat Distance must be a number!");
                    return;
                }
            }

            try
            {
                MidiSequence sequence = MidiSequence.Open(File.OpenRead(args[0]));
                ticks = (double) sequence.Division;

                foreach (MidiTrack track in sequence)
                {
                    track.Events.ConvertDeltasToTotals();
                    foreach (MidiEvent ev in track.Events)
                    {
                        if (ev.GetType().ToString() == "MidiSharp.Events.Voice.Note.OnNoteVoiceMidiEvent")
                        {
                            NoteVoiceMidiEvent nvme = ev as NoteVoiceMidiEvent;
                            note = (double) nvme.Note;
                            time = (double) ev.DeltaTime;

                            Console.WriteLine(((time / ticks) * beatDistance) + "," + ((note - 60.0d) * toneDistance));
                            if (lines.Count >= 2) {
                                lines.Add("  " + ((time / ticks) * beatDistance) + "," + ((note - 60.0d) * toneDistance) + ",0 1 1 0");
                            }
                            else
                            {
                                lines.Add(((time / ticks) * beatDistance) + "," + ((note - 60.0d) * toneDistance) + ",0 1 1 0");
                            }
                        }
                    }
                }

                System.IO.File.WriteAllLines(path, lines);
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine("Error: {0}", exc.Message);
            }
        }
    }
}
