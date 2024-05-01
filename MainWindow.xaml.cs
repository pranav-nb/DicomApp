using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using FellowOakDicom;
using System.Windows.Forms;

namespace DicomApp
{
    public partial class MainWindow : Window
    {
        private readonly List<DicomAttributeInfo> attributeList = new List<DicomAttributeInfo>();
        private readonly List<string> selectedFolders = new List<string>(); // List to store paths of selected folders

        public MainWindow()
        {
            InitializeComponent();
        }

        public static string[] OpenFolderBrowserDialog()
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "Select one or more DICOM folders";
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = Environment.CurrentDirectory;
            fbd.ShowDialog();

            return Directory.GetDirectories(fbd.SelectedPath);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] folders = OpenFolderBrowserDialog();
            foreach (var folder in folders)
            {
                selectedFolders.Add(folder);
                lb.Items.Add(folder);
            }
            DisplayBasicInfo();
        }

        private void DisplayBasicInfo()
        {
            attributeList.Clear(); // Clear previous attributes

            foreach (var folder in selectedFolders)
            {
                try
                {
                    string[] dicomFiles = Directory.GetFiles(folder, "*.dcm", SearchOption.AllDirectories);

                    foreach (var file in dicomFiles)
                    {
                        var dataset = DicomFile.Open(file).Dataset;

                        attributeList.Add(new DicomAttributeInfo
                        {
                            PatientName = dataset.GetString(DicomTag.PatientName),
                            Modality = dataset.GetString(DicomTag.Modality),
                            SeriesDescription = dataset.GetString(DicomTag.SeriesDescription),
                            StudyInstanceUID = dataset.GetString(DicomTag.StudyInstanceUID),
                            SOPClassUID = dataset.GetString(DicomTag.SOPClassUID)
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Error processing DICOM files in folder {folder}: {ex.Message}");
                }
            }

            // Display attributes in the DataGrid
            attributesDataGrid.ItemsSource = attributeList;
        }


        private void ExportToCSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (attributeList.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No DICOM attributes to export.");
                return;
            }

            var csvContent = new StringBuilder();
            csvContent.AppendLine("PatientName,Modality,StudyInstanceUID,SOPClassUID,SeriesDescription");

            foreach (var attributeInfo in attributeList)
            {
                csvContent.AppendLine($"{attributeInfo.PatientName},{attributeInfo.Modality},{attributeInfo.StudyInstanceUID},{attributeInfo.SOPClassUID},{attributeInfo.SeriesDescription}");
            }

            var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Set initial directory
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, csvContent.ToString());
                System.Windows.Forms.MessageBox.Show("DICOM attributes exported to CSV successfully.");
            }
        }


        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a search term.");
                return;
            }

            // Check if the search term is a DICOM tag
            DicomTag tag = DicomTag.Parse(searchTerm);
            if (tag != null)
            {
                // If the search term is a DICOM tag, perform search by tag
                DICOMSearch.SearchByTag(selectedFolders, searchTerm, attributesDataGrid);
            }
            else
            {
                // If the search term is not a DICOM tag, perform search by text
                DICOMSearch.SearchByText(selectedFolders, searchTerm, attributesDataGrid);
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "Enter search term")
            {
                searchTextBox.Text = "";
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                searchTextBox.Text = "Enter search term";
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the selected folders list
            selectedFolders.Clear();

            // Clear the search text box
            searchTextBox.Text = "Enter search term";

            // Clear the attribute list
            attributeList.Clear();
            attributesDataGrid.ItemsSource = null;

            // Show a message indicating that everything has been reset
            System.Windows.Forms.MessageBox.Show("Everything has been reset.");
        }

        private void SearchByTextButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a search term.");
                return;
            }

            DICOMSearch.SearchByText(selectedFolders, searchTerm, attributesDataGrid);
        }

        private void SearchByTagButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a search term.");
                return;
            }

            DicomTag tag = DicomTag.Parse(searchTerm);
            if (tag == null)
            {
                System.Windows.Forms.MessageBox.Show("Invalid DICOM tag format. Please enter a valid DICOM tag (e.g., (0010,0010)).");
                return;
            }

            DICOMSearch.SearchByTag(selectedFolders, searchTerm, attributesDataGrid);
        }
    }
}
