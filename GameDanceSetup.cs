using Il2Cpp;
using MelonLoader;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Decoders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MiSideMania
{
    public class GameDanceSetup
    {
        public static void Setup(string pathToMapsFolder, Location7_GameDance game)
        {
            // All .osu files
            var files = Directory.GetFiles(pathToMapsFolder, "*.osu", SearchOption.AllDirectories);

            // Debug
            MelonLogger.Msg($"path: {pathToMapsFolder}");
            MelonLogger.Msg($"files: {files.Length}");

            // Current musics list
            var musics = game.music.ToList();

            // Populate musics
            foreach (var item in files)
            {
                // IO
                var dir = Directory.GetParent(item)!.FullName;
                var osuFile = item;
                var data = BeatmapDecoder.Decode(osuFile);
                var audioFile = dir + @"\" + data.GeneralSection.AudioFilename;

                // Raw data
                var index = musics.Count;
                var rawColor = data.ColoursSection.ComboColours.FirstOrDefault();
                var color = rawColor != default ? (Color)new Color32(rawColor.R, rawColor.G, rawColor.B, rawColor.A) : musics[Random.Range(0, musics.Count)].colorMusic;
                var beatLengths = data.TimingPoints.Where(x => x.BeatLength > 0).ToArray();
                var speed = (beatLengths.Sum(x => 60000.0 / x.BeatLength) / beatLengths.Length) / 120.0; // 120 BPM = 1.0x speed

                // Debug
                MelonLogger.Msg($"index: {index}");
                MelonLogger.Msg($"dir: {dir}");
                MelonLogger.Msg($"osu: {osuFile}");
                MelonLogger.Msg($"audio: {audioFile}");
                MelonLogger.Msg($"length: {data.GeneralSection.Length / 1000f}s");
                MelonLogger.Msg($"notes: {data.HitObjects.Count}");
                MelonLogger.Msg($"color: {color}");
                MelonLogger.Msg($"speed: {speed}");

                // New data
                var music_data = new Location7_GameDance_Music();
                var notes = new List<Location7_GameDance_Music_Note>();
                var animNoteMita = new AnimationCurve();
                var animNotePlayer = new AnimationCurve();

                // Populate Notes
                foreach (var note in data.HitObjects)
                {
                    var time = (float)note.StartTimeSpan.TotalSeconds;
                    if (time > 0f)
                        notes.Add(new Location7_GameDance_Music_Note() { time = time, side = (note as ManiaNote)!.GetColumn(3) });
                }

                // Set data
                music_data.notes = notes.ToArray();
                music_data.soundTapNo = musics[0].soundTapNo;
                music_data.colorMusic = color;
                music_data.particleMenu = musics[Random.Range(0, musics.Count)].particleMenu;
                music_data.indexText = musics[0].indexText;
                music_data.music = music_data.musicLoop = AudioImportLib.API.LoadAudioClip(audioFile);
                music_data.jumpSlow = 0.5f;
                music_data.addTimeForMita = 0.2f * ((float)speed * 2f);
                music_data.minusTimeClick = -0.2f * ((float)speed * 2f);
                music_data.speedAnimationMita = 1.0f;

                // Set note animations
                animNoteMita.AddKey(game.music[0].animationNoteMita.GetKey(0));
                Keyframe animNoteMitaKey = game.music[0].animationNoteMita.GetKey(1);
                animNoteMitaKey.time = 1f / (float)speed;
                animNoteMita.AddKey(animNoteMitaKey);

                animNotePlayer.AddKey(game.music[0].animationNotePlayer.GetKey(0));
                Keyframe animNotePlayerKey = game.music[0].animationNotePlayer.GetKey(1);
                animNotePlayerKey.time = 1f / (float)speed;
                animNotePlayer.AddKey(animNotePlayerKey);

                music_data.animationNoteMita = animNoteMita;
                music_data.animationNotePlayer = animNotePlayer;

                // Add to game musics list
                musics.Add(music_data);
            }

            // Set new musics list
            game.music = musics.ToArray();
        }
    }
}
