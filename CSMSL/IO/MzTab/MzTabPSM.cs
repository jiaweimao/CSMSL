﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CSMSL.IO.MzTab
{
    public class MzTabPSM : MzTabEntity
    {
        public static class Fields
        {
            public const string Sequence = "sequence";
            public const string ID = "PSM_ID";
            public const string Accession = "accession";
            public const string Unique = "unique";
            public const string Database = "database";
            public const string DatabaseVersion = "database_version";
            public const string SearchEngine = "search_engine";
            public const string SearchEngineScore = "search_engine_score[]";
            public const string Reliability = "reliability";
            public const string Modifications = "modifications";
            public const string RetentionTime = "retention_time";
            public const string Charge = "charge";
            public const string ExperimentalMZ = "exp_mass_to_charge";
            public const string TheoreticalMZ = "calc_mass_to_charge";
            public const string Uri = "uri";
            public const string SpectraReference = "spectra_ref";
            public const string PreviousAminoAcid = "pre";
            public const string FollowingAminoAcid = "post";
            public const string StartResidue = "start";
            public const string EndResidue = "end";

            internal static IEnumerable<string> GetHeader(IList<MzTabPSM> psms)
            {
                List<string> headers = new List<string>();
                headers.Add(Sequence);
                headers.Add(ID);
                headers.Add(Accession);
                headers.Add(Unique);
                headers.Add(Database);
                headers.Add(DatabaseVersion);
                headers.Add(SearchEngine);

                headers.AddRange(GetHeaders(psms, SearchEngineScore, (psm => psm.SearchEngineScores)));

                // Only report reliability if one psm has a non-null reliability score
                if (psms.Any(psm => psm.Reliability != MzTab.ReliabilityScore.NotSet))
                    headers.Add(Reliability);

                headers.Add(Modifications);
                headers.Add(RetentionTime);
                headers.Add(Charge);
                headers.Add(ExperimentalMZ);
                headers.Add(TheoreticalMZ);

                if (psms.Any(psm => psm.Uri != null))
                    headers.Add(Uri);

                headers.Add(SpectraReference);
                headers.Add(PreviousAminoAcid);
                headers.Add(FollowingAminoAcid);
                headers.Add(StartResidue);
                headers.Add(EndResidue);

                // Optional Parameters
                headers.AddRange(psms.Where(psm => psm.GetOptionalFields() != null).SelectMany(psm => psm.GetOptionalFields()));

                return headers;
            }
        }
        
        /// <summary>
        /// The peptide's sequence corresponding to the PSM
        /// </summary>
        public string Sequence { get; set; }

        public int ID { get; set; }

        public string Accession{ get; set; }
    
        public bool Unique{ get; set; }
  
        public string Database{ get; set; }
   
        public string DatabaseVersion{ get; set; }
     
        public List<CVParamater> SearchEngines{ get; set; }

        private List<double> _searchEngineScores;
        public List<double> SearchEngineScores
        {
            get { return _searchEngineScores; }
            set { _searchEngineScores = value; }
        }

        public MzTab.ReliabilityScore Reliability{ get; set; }
            
        public string Modifications { get; set; }
     
        public List<double> RetentionTime { get; set; }
    
        public int Charge { get; set; }
     
        public double ExperimentalMZ { get; set; }
     
        public double TheoreticalMZ { get; set; }
      
        public Uri Uri { get; set; }
      
        public string SpectraReference { get; set; }
     
        public char PreviousAminoAcid { get; set; }
    
        public char FollowingAminoAcid { get; set; }
      
        public int EndResiduePosition { get; set; }

        public int StartResiduePosition { get; set; }
       
        public MzTabPSM()
            : base(18) { }

        public override string ToString()
        {
            return string.Format("(#{0}) {1}", ID, Sequence);
        }

        public override string GetValue(string fieldName)
        {
            switch (fieldName)
            {
                case Fields.Sequence:
                    return Sequence;
                case Fields.ID:
                    return ID.ToString();
                case Fields.Accession:
                    return Accession;
                case Fields.Unique:
                    return Unique ? "1" : "0";
                case Fields.Database:
                    return Database;
                case Fields.DatabaseVersion:
                    return DatabaseVersion;
                case Fields.SearchEngine:
                    return string.Join("|", SearchEngines);
                case Fields.Reliability:
                    if (Reliability == MzTab.ReliabilityScore.NotSet)
                        return MzTab.NullFieldText;
                    return ((int)Reliability).ToString();
                case Fields.Modifications:
                    return Modifications;
                case Fields.RetentionTime:
                    return string.Join("|", RetentionTime);
                case Fields.Charge:
                    return Charge.ToString();
                case Fields.ExperimentalMZ:
                    return ExperimentalMZ.ToString();
                case Fields.TheoreticalMZ:
                    return TheoreticalMZ.ToString();
                case Fields.Uri:
                    return Uri.ToString();
                case Fields.SpectraReference:
                    return SpectraReference;
                case Fields.PreviousAminoAcid:
                    return PreviousAminoAcid.ToString();
                case Fields.FollowingAminoAcid:
                    return FollowingAminoAcid.ToString();
                case Fields.StartResidue:
                    return StartResiduePosition.ToString();
                case Fields.EndResidue:
                    return EndResiduePosition.ToString();
                default:
                    if (fieldName.Contains("["))
                    {
                        string condensedFieldName;
                        List<int> indices = MzTab.GetFieldIndicies(fieldName, out condensedFieldName);

                        if (condensedFieldName == Fields.SearchEngineScore)
                        {
                            return GetListValue(_searchEngineScores, indices[0]);
                        }
                    }
                    else if (fieldName.StartsWith(MzTab.OptionalColumnPrefix))
                    {
                        // handle optional parameters
                    } 
                    return MzTab.NullFieldText;
            }
        }
        
        public override void SetValue(string fieldName, string value)
        {
            switch (fieldName)
            {
                case Fields.Sequence:
                    Sequence = value; return;
                case Fields.ID:
                    ID = int.Parse(value); return;
                case Fields.Accession:
                    Accession = value; return;
                case Fields.Unique:
                    Unique = value.Equals("1"); return;
                case Fields.Database:
                    Database = value; return;
                case Fields.DatabaseVersion:
                    DatabaseVersion = value; return;
                case Fields.SearchEngine:
                    SearchEngines = value.Split('|').Select(datum => (CVParamater)datum).ToList(); return;
                case Fields.Reliability:
                    Reliability = (MzTab.ReliabilityScore)int.Parse(value); return;
                case Fields.Modifications:
                    Modifications = value; return;
                case Fields.RetentionTime:
                    RetentionTime = value.Split('|').Select(double.Parse).ToList(); return;
                case Fields.Charge:
                    Charge = int.Parse(value); return;
                case Fields.ExperimentalMZ:
                    ExperimentalMZ = double.Parse(value); return;
                case Fields.TheoreticalMZ:
                    TheoreticalMZ = double.Parse(value); return;
                case Fields.Uri:
                    Uri = new Uri(value); return;
                case Fields.SpectraReference:
                    SpectraReference = value; return;
                case Fields.PreviousAminoAcid:
                    PreviousAminoAcid = value[0]; return;
                case Fields.FollowingAminoAcid:
                    FollowingAminoAcid = value[0]; return;
                case Fields.StartResidue:
                    StartResiduePosition = int.Parse(value); return;
                case Fields.EndResidue:
                    EndResiduePosition = int.Parse(value); return;
                default:
                    if (fieldName.Contains("["))
                    {
                        string condensedFieldName;
                        List<int> indices = MzTab.GetFieldIndicies(fieldName, out condensedFieldName);

                        if (condensedFieldName == Fields.SearchEngineScore)
                        {
                            SetRawValue(ref _searchEngineScores, indices[0], double.Parse(value));
                            return;
                        }
                    } else if (fieldName.StartsWith(MzTab.OptionalColumnPrefix)) {
                        // handle optional parameters
                    } 

                    throw new ArgumentException("Unexpected field name: "+ fieldName);
            }
        }
        
    }
}
