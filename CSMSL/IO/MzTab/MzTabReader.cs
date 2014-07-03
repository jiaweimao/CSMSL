﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace CSMSL.IO.MzTab
{
    public sealed class MzTabReader : IDisposable
    {
        #region Public Properties

        public string FilePath { get; private set; }
        public bool IsOpen { get; private set; }
        public MzTab.MzTabMode Mode { get; private set; }
        public MzTab.MzTabType Type { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }

        #endregion

        #region Modifications

        private DataTable _modDataTable;

        #endregion

        #region ExperimentalSetup

        private DataTable _msRunLocationDataTable;

        #endregion
        
        private StreamReader _reader;
        private MzTab.States _currentState;
        private readonly DataSet _dataSet;
     
        #region Constructors

        public MzTabReader(string filePath, bool ignoreComments = true)
        {
            IsOpen = false;
            FilePath = filePath;
            _ignoreComments = ignoreComments;
            _dataSet = new DataSet(FilePath) {CaseSensitive = MzTab.CaseSensitive};
            _metaDataTable = _dataSet.Tables.Add("MetaData");
            _metaDataTable.Columns.Add("key");
            _metaDataTable.Columns.Add("value");
            _modDataTable = _dataSet.Tables.Add("Modifications");
            _modDataTable.Columns.Add("key");
            _modDataTable.Columns.Add("value");
            _modDataTable.Columns.Add("isFixed", typeof(bool));
            _msRunLocationDataTable = _dataSet.Tables.Add("MS Runs");
            _msRunLocationDataTable.Columns.Add("key");
            _msRunLocationDataTable.Columns.Add("value");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens up the mzTab file and parses all the information into memory
        /// </summary>
        public void Open()
        {
            var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _reader = new StreamReader(stream, MzTab.DefaultEncoding, true);
            IsOpen = true;
            Read();
        }
        
        public void Dispose()
        {
            if (_reader != null)
                _reader.Dispose();
            IsOpen = false;
        }

        public string[] GetColumns(MzTabSection section)
        {
            switch (section)
            {
                case MzTabSection.PSM:
                    return _psmDataTable != null ? _psmDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray() : null;
                case MzTabSection.Peptide:
                    return _peptideDataTable != null ? _peptideDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray() : null;
                case MzTabSection.Protein:
                    return _proteinDataTable != null ? _proteinDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray() : null;
                case MzTabSection.SmallMolecule:
                    return _smallMoleculeDataTable != null ? _smallMoleculeDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray() : null;
                default:
                    return null;
            }
        }

        public bool ContainsColumn(MzTabSection section, string columnName)
        {
            switch (section)
            {
                case MzTabSection.PSM:
                    return _psmDataTable != null && _psmDataTable.Columns.Contains(columnName);
                case MzTabSection.Peptide:
                    return _peptideDataTable != null && _peptideDataTable.Columns.Contains(columnName);
                case MzTabSection.Protein:
                    return _proteinDataTable != null && _proteinDataTable.Columns.Contains(columnName);
                case MzTabSection.SmallMolecule:
                    return _smallMoleculeDataTable != null && _smallMoleculeDataTable.Columns.Contains(columnName);
                default:
                    return false;
            }
        }

        public string GetData(MzTabSection section, int index, string columnName)
        {
            switch (section)
            {
                case MzTabSection.PSM:
                    return _psmDataTable != null ? (string)_psmDataTable.Rows[index][columnName] : null;
                case MzTabSection.Peptide:
                    return _peptideDataTable != null ? (string)_peptideDataTable.Rows[index][columnName] : null;
                case MzTabSection.Protein:
                    return _proteinDataTable != null ? (string)_proteinDataTable.Rows[index][columnName] : null;
                case MzTabSection.SmallMolecule:
                    return _smallMoleculeDataTable != null ? (string)_smallMoleculeDataTable.Rows[index][columnName] : null;
                default:
                    return null;
            }
        }

        public string[] GetData(MzTabSection section, int index)
        {
            switch (section)
            {
                case MzTabSection.PSM:
                    return _psmDataTable != null ? (string[])_psmDataTable.Rows[index].ItemArray : null;
                case MzTabSection.Peptide:
                    return _peptideDataTable != null ? (string[])_peptideDataTable.Rows[index].ItemArray : null;
                case MzTabSection.Protein:
                    return _proteinDataTable != null ? (string[])_proteinDataTable.Rows[index].ItemArray : null;
                case MzTabSection.SmallMolecule:
                    return _smallMoleculeDataTable != null ? (string[])_smallMoleculeDataTable.Rows[index].ItemArray : null;
                default:
                    return null;
            }
        }

        public string this[MzTabSection section, int index, string columnName]
        {
            get
            {
                return GetData(section, index, columnName);
            }
        }

        public string[] this[MzTabSection section, int index]
        {
            get
            {
                return GetData(section, index);
            }
        }
        
        #endregion

        #region Private Methods

        private void Read()
        {
            int lineNumber = 0;
            while (!_reader.EndOfStream)
            {
                lineNumber++;
                
                // Read the next line
                string line = _reader.ReadLine();

                // Empty lines are ignored
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Split the line into different parts
                string[] data = line.Split(MzTab.FieldSeparator);
               
                // Get the line prefix of the current line
                string linePrefix = data[0];

                // Jump to the method that handles each of the different line prefixes
                switch (linePrefix)
                {
                    // Comments
                    case MzTab.CommentLinePrefix:
                        ReadComment(data, lineNumber);
                        break;

                    // MetaData
                    case MzTab.MetaDataLinePrefix:
                        ReadMetaData(data, lineNumber);
                        break;

                    // Table Headers
                    case MzTab.ProteinTableLinePrefix:
                        _proteinDataTable = _dataSet.Tables.Add(MzTab.ProteinSection);
                        ReadTableDefinition(MzTab.States.ProteinHeader, data, _proteinDataTable);
                        break;
                    case MzTab.PeptideTableLinePrefix:
                        _peptideDataTable = _dataSet.Tables.Add(MzTab.PeptideSection);
                        ReadTableDefinition(MzTab.States.PeptideHeader, data, _peptideDataTable);
                        break;
                    case MzTab.PsmTableLinePrefix:
                        _psmDataTable = _dataSet.Tables.Add(MzTab.PsmSection);
                        ReadTableDefinition(MzTab.States.PsmHeader, data, _psmDataTable);
                        break;
                    case MzTab.SmallMoleculeTableLinePrefix:
                        _smallMoleculeDataTable = _dataSet.Tables.Add(MzTab.SmallMoleculeSection);
                        ReadTableDefinition(MzTab.States.SmallMoleculeHeader, data, _smallMoleculeDataTable);
                        break;
                   
                    // Table Data
                    case MzTab.ProteinDataLinePrefix:
                        ReadDataTable(MzTab.States.ProteinData, data, _proteinDataTable);
                        break;
                    case MzTab.PeptideDataLinePrefix:
                        ReadDataTable(MzTab.States.PeptideData, data, _peptideDataTable);
                        break;
                    case MzTab.PsmDataLinePrefix:
                        ReadDataTable(MzTab.States.PsmData, data, _psmDataTable);
                        break;
                    case MzTab.SmallMoleculeDataLinePrefix:
                        ReadDataTable(MzTab.States.SmallMoleculeData, data, _smallMoleculeDataTable);
                        break;

                    // If we got here, the line prefix is not valid
                    default:
                        CheckError(line, lineNumber);
                        break;
                }
            }
        }
        
        private void CheckError(string line, int lineNumber)
        {
            throw new ArgumentException("Unable to correctly parse line #" + lineNumber);
        }

        private void ReadTableDefinition(MzTab.States headerState, string[] data, DataTable table)
        {
            if ((_currentState & MzTab.States.MetaData) != MzTab.States.MetaData)
            {
                throw new ArgumentException("The MetaData section MUST occur before the " + table.TableName + " Section. Invalid input file");
            }

            if ((_currentState & headerState) == headerState)
            {
                throw new ArgumentException("The " + table.TableName + " Table Header has already been parsed once, only one  " + table.TableName + " section is allowed per mzTab file.");
            }

            // Set the we have entered the current state
            _currentState |= headerState;

            int i = 1;
            while (i < data.Length && !string.IsNullOrWhiteSpace(data[i]))
            {
                table.Columns.Add(data[i].Trim());
                i++;
            }
        }

        private void ReadDataTable(MzTab.States dataState, string[] data, DataTable table)
        {
            if (table == null)
            {
                throw new ArgumentException("No header information loaded for " + dataState + ", unable to parse data");
            }

            // Set the we have entered the current state
            _currentState |= dataState;

            // Add the row to the Protein data table
            table.Rows.Add(data.SubArray(1, table.Columns.Count));
        }
        
        #endregion

        #region Peptide Section
        
        private DataTable _peptideDataTable;

        public bool ContainsPeptides { get { return _peptideDataTable != null && _peptideDataTable.Rows.Count > 0; } }
        public int NumberOfPeptides { get { return (ContainsPeptides) ? _peptideDataTable.Rows.Count : 0; } }
        
        #endregion

        #region Small Molecule Section

        private DataTable _smallMoleculeDataTable;

        public bool ContainsSmallMolecules { get { return _smallMoleculeDataTable != null && _smallMoleculeDataTable.Rows.Count > 0; } }
        public int NumberOfSmallMolecules { get { return (ContainsSmallMolecules) ? _smallMoleculeDataTable.Rows.Count : 0; } }
        
        #endregion

        #region PSM Section

        private DataTable _psmDataTable;

        public bool ContainsPsms { get { return _psmDataTable != null && _psmDataTable.Rows.Count > 0; } }
        public int NumberOfPsms { get { return (ContainsPsms) ? _psmDataTable.Rows.Count : 0; } }

        #endregion

        #region Protein Section

        private DataTable _proteinDataTable;
        
        public bool ContainsProteins { get { return _proteinDataTable != null && _proteinDataTable.Rows.Count > 0; } }
        public int NumberOfProteins { get { return (ContainsProteins) ? _proteinDataTable.Rows.Count : 0; } }
        
        #endregion

        #region Comment Section

        private DataTable _commentsDataTable;
        private readonly bool _ignoreComments;
        public bool ContainsComments { get { return _commentsDataTable != null; } }

        private void ReadComment(string[] data, int lineNumber)
        {
            // Do nothing with the comment if we aren't storing them
            if (_ignoreComments)
                return;

            // Create the comment table if it doesn't exist
            if (_commentsDataTable == null)
            {
                _commentsDataTable = _dataSet.Tables.Add("Comments");
                _commentsDataTable.Columns.Add("lineNumber", typeof(int));
                _commentsDataTable.Columns.Add("comment");
            }

            // The comment should be the second thing in the data array
            string comment = data[1];

            _commentsDataTable.Rows.Add(lineNumber, comment);
        }

        #endregion

        #region MetaData Section

        private readonly DataTable _metaDataTable;

        private void ReadMetaData(string[] data, int lineNumber)
        {
            // Set that we have enter in Metadata section
            _currentState |= MzTab.States.MetaData;
            
            // Grab the key-value pair, which should correspond to index 1 and 2, respectively
            string key = data[1];
            string value = data[2];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("No key was specified in the metadata section at line #" + lineNumber);
            }

            // Handle mandatory metadata keys
            switch (key)
            {
                case MzTab.MDVersionField:
                    Version = value;
                    break;
                case MzTab.MDModeField:
                    Mode = (MzTab.MzTabMode)Enum.Parse(typeof(MzTab.MzTabMode), value, true);
                    break;
                case MzTab.MDTypeField:
                    Type = (MzTab.MzTabType)Enum.Parse(typeof(MzTab.MzTabType), value, true);
                    break;
                case MzTab.MDDescriptionField:
                    Description = value;
                    break;
                case MzTab.MDMsRunLocationField:
                    _msRunLocationDataTable.Rows.Add(key, value);
                    break;
                case MzTab.MDFixedModField:
                    _modDataTable.Rows.Add(key, value, true);
                    break;
                case MzTab.MDVariableModField:
                    _modDataTable.Rows.Add(key, value, false);
                    break;
            }

            // Add the data to the MetaData table
            _metaDataTable.Rows.Add(key, value);
        }

        #endregion
        
        //public string this[MzTab.LinePrefix prefix, int index, string field]
        //{
        //    get
        //    {
        //        return _psmDataTable.Rows[index][field] as string;
        //    }
        //}

        //public object[] this[MzTab.LinePrefix prefix, int index]
        //{
        //    get
        //    {
        //        return _psmDataTable.Rows[index].ItemArray;
        //    }
        //}
       
        public IEnumerable<MzTabPSM> GetPsms()
        {
            if (_psmDataTable == null)
                yield break;

            bool containsReliability = _psmDataTable.Columns.Contains(MzTab.PSMRelibailityField);
            bool containsUri = _psmDataTable.Columns.Contains(MzTab.PSMUriField);

        
            foreach (DataRow row in _psmDataTable.Rows)
            {
                MzTabPSM psm = new MzTabPSM
                {
                    Sequence = (string)row[MzTab.PSMSequenceField],
                    ID = int.Parse((string)row[MzTab.PSMIdField])
                };

                if(containsReliability)
                    psm.Reliability = int.Parse((string)row[MzTab.PSMRelibailityField]);

                if(containsUri)
                    psm.URI = (string)row[MzTab.PSMUriField];
                
                yield return psm;
            }
        }
    }
}
