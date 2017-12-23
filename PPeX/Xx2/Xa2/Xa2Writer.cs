using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class Xa2Writer
    {
        public void Write(BinaryWriter writer, XaFile file)
        {
            WriteSection5(writer, file.Section5);
        }

        protected void WriteSection5(BinaryWriter writer, XaSection5 section)
        {
            writer.Write(section.Clips.Length);

            ////clip -> name
            //foreach (string name in section.Clips.Select(x => x.Name))
            //    writer.WriteEncryptedStringRaw(name);

            ////clip -> start
            //foreach (float start in section.Clips.Select(x => x.Start))
            //    writer.Write(start);

            ////clip -> end
            //foreach (float end in section.Clips.Select(x => x.End))
            //    writer.Write(end);

            ////clip -> speed
            //foreach (float speed in section.Clips.Select(x => x.Speed))
            //    writer.Write(speed);

            ////clip -> next
            //foreach (float next in section.Clips.Select(x => x.Next))
            //    writer.Write(next);

            ////clip -> unknowns
            //for (int i = 0; i < 7; i++)
            //    foreach (byte[] unknown in section.Clips.Select(x => x.Unknowns[i]))
            //        writer.Write(unknown);

            foreach (var clip in section.Clips)
            {
                writer.WriteEncryptedStringRaw(clip.Name);
                writer.Write(clip.Start);
                writer.Write(clip.End);
                writer.Write(clip.Speed);
                writer.Write(clip.Next);

                foreach (byte[] unknown in clip.Unknowns)
                    writer.Write(unknown);
            }

            writer.Write(section.Tracks.Length);

            //track -> name
            foreach (var name in section.Tracks.Select(x => x.Name))
                writer.Write(name);

            //track -> unknown
            foreach (var unknown in section.Tracks.Select(x => x.Unknown))
                writer.Write(unknown);

            //track -> keyframes
            foreach (var keyframes in section.Tracks.Select(x => x.Keyframes))
                writer.Write(keyframes.Length);

            //track -> keyframes -> index (delta)
            foreach (var keyframes in section.Tracks.Select(x => x.Keyframes))
            {
                int last = 0;
                writer.Write(keyframes.Length);

                foreach (var index in keyframes.Select(x => x.Index))
                {
                    int delta = index - last;

                    writer.Write(delta);

                    last = index;
                }
            }

            //track -> keyframe -> rotation
            foreach (var keyframes in section.Tracks.Select(x => x.Keyframes))
                foreach (float[] rotation in keyframes.Select(x => x.Rotation))
                    for (int i = 0; i < 4; i++)
                        writer.Write(rotation[i]);

            //track -> keyframe -> translation
            foreach (var keyframes in section.Tracks.Select(x => x.Keyframes))
                foreach (float[] translation in keyframes.Select(x => x.Translation))
                    for (int i = 0; i < 3; i++)
                        writer.Write(translation[i]);

            //track -> keyframe -> scaling
            foreach (var keyframes in section.Tracks.Select(x => x.Keyframes))
                foreach (float[] scaling in keyframes.Select(x => x.Scaling))
                    for (int i = 0; i < 3; i++)
                        writer.Write(scaling[i]);

            //track -> keyframe -> unknown
            foreach (var keyframes in section.Tracks.Select(x => x.Keyframes))
                foreach (var unknown in keyframes.Select(x => x.Unknown))
                    writer.Write(unknown);



            foreach (var track in section.Tracks)
            {
                //writer.Write(track.Name);
                //writer.Write(track.Unknown);

                foreach (var frame in track.Keyframes)
                {
                    //writer.Write(frame.Index);

                    //for (int i = 0; i < 4; i++)
                    //    writer.Write(frame.Rotation[i]);

                    //for (int i = 0; i < 3; i++)
                    //    writer.Write(frame.Translation[i]);

                    //for (int i = 0; i < 3; i++)
                    //    writer.Write(frame.Scaling[i]);

                    //writer.Write(frame.Unknown);
                }
            }
        }
    }
}
