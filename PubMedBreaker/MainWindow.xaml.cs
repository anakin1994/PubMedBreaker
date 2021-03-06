﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QueryHandler;
using System.Threading.Tasks;

namespace PubMedBreaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly UserQueryHandler _uqh = new UserQueryHandler();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteQuery();
        }
        
        private void ListViewResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != ListViewResults)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    // Do something here
                    var id = ((obj as ListViewItem).DataContext as PubMedArticleResult).PubMedId;
                    System.Diagnostics.Process.Start("http://www.ncbi.nlm.nih.gov/pubmed/" + id);
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private async void TextBoxQuery_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // your event handler here
                e.Handled = true;
                await ExecuteQuery();
            }
        }

        private async Task ExecuteQuery()
        {
            Button.IsEnabled = false;
            TextBoxSynonyms.Text = String.Empty;
            TextBoxUnifiedQuery.Text = String.Empty;
            ListViewResults.ItemsSource = null;
            StatusLabel.Content = String.Empty;

            try
            {
                string userQuery = TextBoxQuery.Text;
                int resultsNumber = 0;
                if (IntegerUpDownResultNumber.Value != null)
                {
                    resultsNumber = IntegerUpDownResultNumber.Value.Value;
                }

                FinalResultsSet results = await _uqh.GetResultsForQuery(userQuery, resultsNumber);

                TextBoxUnifiedQuery.Text = results.UnifiedQuery;

                foreach (var synonym in results.Synonyms)
                {
                    TextBoxSynonyms.Text += synonym + Environment.NewLine;
                }

                ListViewResults.ItemsSource = results.UserQueryResults;

                StatusLabel.Content =
                    $"Wykonano zapytania w: {results.ExecutionTimeMilis} ms";
            }
            catch (Exception exc)
            {
                StatusLabel.Content = exc.Message;
            }
            Button.IsEnabled = true;
        }
    
    }
}
