using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using FellowOakDicom;
using System.Windows.Controls;

namespace DicomApp
{
    public static class DICOMSearch
    {

        public static void SearchByText(List<string> selectedFolders, string searchTerm, DataGrid attributesDataGrid)
        {
            // List to store search results
            List<DicomAttributeInfo> searchResults = new List<DicomAttributeInfo>();

            foreach (var folder in selectedFolders)
            {
                try
                {
                    // Get all DICOM files within the folder and its subfolders
                    string[] dicomFiles = Directory.GetFiles(folder, "*.dcm", SearchOption.AllDirectories);

                    foreach (var file in dicomFiles)
                    {
                        var dataset = DicomFile.Open(file).Dataset;

                        // Extract DICOM attributes
                        string patientName = dataset.GetString(DicomTag.PatientName);
                        string modality = dataset.GetString(DicomTag.Modality);
                        string seriesDescription = dataset.GetString(DicomTag.SeriesDescription);
                        string studyInstanceUID = dataset.GetString(DicomTag.StudyInstanceUID);
                        string sopClassUID = dataset.GetString(DicomTag.SOPClassUID);

                        // Check if the search term matches any DICOM tag values
                        if (patientName.Contains(searchTerm) ||
                            modality.Contains(searchTerm) ||
                            seriesDescription.Contains(searchTerm) ||
                            studyInstanceUID.Contains(searchTerm) ||
                            sopClassUID.Contains(searchTerm))
                        {
                            // Add the DICOM attributes to searchResults if the search term is found
                            searchResults.Add(new DicomAttributeInfo
                            {
                                PatientName = patientName,
                                Modality = modality,
                                SeriesDescription = seriesDescription,
                                StudyInstanceUID = studyInstanceUID,
                                SOPClassUID = sopClassUID
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing DICOM files in folder {folder}: {ex.Message}");
                }
            }

            // Display or handle search results as needed
            if (searchResults.Count > 0)
            {
                // If search results are found, display them in the DataGrid
                attributesDataGrid.ItemsSource = searchResults;
                MessageBox.Show($"Found {searchResults.Count} matching results.");
            }
            else
            {
                // If no search results are found, notify the user
                MessageBox.Show("No matching results found.");
            }
        }


        public static void SearchByTag(List<string> selectedFolders, string searchTerm, DataGrid attributesDataGrid)
        {
            // List to store search results
            List<DicomAttributeInfo> searchResults = new List<DicomAttributeInfo>();

            foreach (var folder in selectedFolders)
            {
                try
                {
                    // Get all DICOM files within the folder and its subfolders
                    string[] dicomFiles = Directory.GetFiles(folder, "*.dcm", SearchOption.AllDirectories);

                    foreach (var file in dicomFiles)
                    {
                        var dataset = DicomFile.Open(file).Dataset;

                        // Search for the specified DICOM tag value
                        DicomTag tag = DicomTag.Parse(searchTerm);
                        if (tag != null)
                        {
                            string tagValue = dataset.GetString(tag);
                            if (!string.IsNullOrEmpty(tagValue))
                            {
                                searchResults.Add(new DicomAttributeInfo
                                {
                                    Tag = tag.ToString(),
                                    Value = tagValue
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing DICOM files in folder {folder}: {ex.Message}");
                }
            }

            // Display or handle search results as needed
            if (searchResults.Count > 0)
            {
                // If search results are found, display them in the DataGrid
                attributesDataGrid.ItemsSource = searchResults;
                MessageBox.Show($"Found {searchResults.Count} matching results.");
            }
            else
            {
                // If no search results are found, notify the user
                MessageBox.Show("No matching results found.");
            }
        }

    }
}
