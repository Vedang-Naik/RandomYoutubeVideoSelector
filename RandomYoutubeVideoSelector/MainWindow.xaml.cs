using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RandomYoutubeVideoSelector
{
    public partial class MainWindow : Window
    {
        private JObject SongInfoBank;
        private const string SongInfoBankPath = @"C:\Users\Vedang Naik\Desktop\Programming\RandomYoutubeVideoSelector\SongInfoBank.json";
        private const string YoutubeURL = "https://www.youtube.com/watch_videos?video_ids=";

        /// <summary>
        /// The constructor for the Window. It opens the file and keeps it open for the duration of the program, as well as writing a default value
        /// to the NumSongs textbox.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            using (StreamReader file = File.OpenText(SongInfoBankPath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                SongInfoBank = (JObject)JToken.ReadFrom(reader);
            }
            NumSongsTextBox.Text = (SongInfoBank.Count / 2).ToString();
        }

        /// <summary>
        /// This function makes sure that the NumSongs textbox cannot take in strings.
        /// </summary>
        private void CheckNumSongs(Object sender, TextCompositionEventArgs e)
        {
            Regex _ = new Regex("[^0-9]+");
            e.Handled = _.IsMatch(e.Text);
        }
        
        /// <summary>
        /// This function handles the Click event for the "Play Songs" button. This button selects a random subset of a size given by the user
        /// from SongInfoBank.json and then opens a YouTube tab to play them.
        /// </summary>
        private void HandleClickPlay(Object sender, RoutedEventArgs e)
        {
            // Ensure that a blank string cannot be passed in.
            if (string.IsNullOrWhiteSpace(NumSongsTextBox.Text))
            {
                NotifLabel.Text = "Please enter a number.";
                return;
            }

            // Clear the notifications label.
            NotifLabel.Text = "";

            // If the number of songs chosen by the user is more than the number of songs in the list, clamp it to the list's length.
            int NumSongsToPlay = Math.Min(SongInfoBank.Count, Int32.Parse(NumSongsTextBox.Text));
            string SongURLs = "";

            // 1. Convert the keys into an IList so that a random subset of them can be selected.
            // 2. In the first for loop, randomly switch the ith element with any of the elements after it, in place.
            // 3. In the second loop, take the first NumSongsToPlay of them, which will now be random.
            IList<string> SongKeys = SongInfoBank.Properties().Select(p => p.Name).ToList();
            for (int i = 0; i < NumSongsToPlay; i++)
            {                
                // When getting a random number from i + 1 to the end of the list, at the end of the list, length + 1 is chosen.
                // Thus, the index is clamped to the last index of the list.
                int RandomIndex = Math.Min(new Random().Next(i + 1, SongInfoBank.Count), SongInfoBank.Count - 1);

                string temp = SongKeys[i];
                SongKeys[i] = SongKeys[RandomIndex];
                SongKeys[RandomIndex] = temp;
            }
            for (int i = 0; i < NumSongsToPlay; i++)
            {
                // Get the ID of the video using the key at i and add it to SongURLs. 
                SongURLs += SongInfoBank[SongKeys[i]] + ",";
            }

            // Start Chrome with the constructed URL.
            Process.Start("chrome.exe", YoutubeURL + SongURLs);
        }

        /// <summary>
        /// This function handles the Click event for the "Add Song" button. It adds a new key-value pair of the song name and ID to SongInfoBank.json.
        /// </summary>
        private void HandleClickAdd(Object sender, RoutedEventArgs e)
        {
            // Make sure that the SongID and SongName textboxes are not blank.
            if (string.IsNullOrWhiteSpace(SongNameTextBox.Text) || string.IsNullOrWhiteSpace(SongIDTextBox.Text))
            {
                NotifLabel.Text = "Enter a song name and/or ID.";
                return;
            }

            // Clear the notifications label.
            NotifLabel.Text = "";

            // Add the new key-value pair to the json file. If the key already exists, the ID is overwritten. This need to be fixed.
            SongInfoBank[SongNameTextBox.Text] = SongIDTextBox.Text;

            // Write the file to disk. This overwrites the old file, so O(n) might be a problem.
            using (StreamWriter file = File.CreateText(SongInfoBankPath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                SongInfoBank.WriteTo(writer);
            }
        }
    }
}