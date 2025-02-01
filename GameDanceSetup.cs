using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Decoders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MiSideMania
{
    public class GameDanceSetup : MonoBehaviour
    {
        private const float FLASH_STRENGTH_T = 0.1f;
        private const float MIN_SECONDS_BETWEEN_NOTES = 0.1f;

        public static void Setup(string pathToMapsFolder, Location7_GameDance game)
        {
            ClassInjector.RegisterTypeInIl2Cpp<GameDanceSetup>();

            // Decrease flashes
            for (int i = 0; i < 3; i++)
                game.colorDanceSide[i] = Color.Lerp(game.colorDance, game.colorDanceSide[i], FLASH_STRENGTH_T);

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

                if (data.GeneralSection.Mode != OsuParsers.Enums.Ruleset.Mania)
                    continue;

                // Raw data
                var index = musics.Count;
                var rawColor = data.ColoursSection.ComboColours.FirstOrDefault();
                var color = rawColor != default ? (Color)new Color32(rawColor.R, rawColor.G, rawColor.B, rawColor.A) : new Color(Random.Range(0f, 0.75f), Random.Range(0f, 0.75f), Random.Range(0f, 0.75f));

                // Debug
                MelonLogger.Msg($"index: {index}");
                MelonLogger.Msg($"dir: {dir}");
                MelonLogger.Msg($"osu: {osuFile}");
                MelonLogger.Msg($"audio: {audioFile}");
                MelonLogger.Msg($"length: {data.GeneralSection.Length / 1000f}s");
                MelonLogger.Msg($"notes: {data.HitObjects.Count}");
                MelonLogger.Msg($"color: {color}");

                // New data
                var music_data = new Location7_GameDance_Music();
                var notes = new List<Location7_GameDance_Music_Note>();

                // Populate notes
                float prevTime = 0f;
                foreach (var note in data.HitObjects.OrderBy(x => x.StartTimeSpan.TotalSeconds))
                {
                    var time = (float)note.StartTimeSpan.TotalSeconds;
                    if (time > 1f && Mathf.Abs(prevTime - time) >= MIN_SECONDS_BETWEEN_NOTES)
                    {
                        notes.Add(new Location7_GameDance_Music_Note() { time = time, side = (note as ManiaNote)!.GetColumn(3) });
                        prevTime = time;
                    }
                }

                // Set data
                music_data.notes = notes.ToArray();
                music_data.soundTapNo = musics[0].soundTapNo;
                music_data.colorMusic = color;
                music_data.particleMenu = musics[Random.Range(0, musics.Count)].particleMenu;
                music_data.indexText = musics[0].indexText;
                music_data.music = music_data.musicLoop = AudioImportLib.API.LoadAudioClip(audioFile, false);
                music_data.music.name = data.MetadataSection.Title + "\n"
                    + $"[ {System.TimeSpan.FromMilliseconds(data.GeneralSection.Length):mm\\:ss} ] [ {data.MetadataSection.Version} ]";
                music_data.jumpSlow = 0.5f;
                music_data.addTimeForMita = 0.2f;
                music_data.minusTimeClick = -0.2f;
                music_data.speedAnimationMita = 1.0f;
                music_data.animationNoteMita = musics[0].animationNoteMita;
                music_data.animationNotePlayer = musics[0].animationNotePlayer;

                // Add to game musics list
                musics.Add(music_data);
            }

            // Set new musics list
            game.music = musics.ToArray();

            game.gameObject.AddComponent<GameDanceSetup>();
        }

        private Text gameName = null!, customName = null!;
        private Location7_GameDance game = null!;

        public void Awake()
        {
            game = GetComponent<Location7_GameDance>();

            game.transform.GetComponentsInChildren<Text>(true).First(x => x.name == "Score").horizontalOverflow = HorizontalWrapMode.Overflow;
            gameName = transform.GetComponentsInChildren<Text>(true).First(x => x.name == "NameMusic");

            customName = Instantiate(gameName, gameName.transform.parent);
            customName.fontSize = 50;

            var rect = customName.GetComponent<RectTransform>();
            rect.localEulerAngles = Vector3.zero;
            rect.offsetMin += new Vector2(100, 0);
            rect.offsetMax -= new Vector2(100, 0);

            customName.gameObject.SetActive(false);
        }

        public void Update()
        {
            gameName.enabled = game.musicIndexPlay < 3;
            if (game.musicIndexPlay > 2)
            {
                customName.gameObject.SetActive(true);
                customName.text = game.music[game.musicIndexPlay].music.name;
                customName.color = game.music[game.musicIndexPlay].colorMusic;
            }
            else
            {
                customName.gameObject.SetActive(false);
            }
        }
    }
}
