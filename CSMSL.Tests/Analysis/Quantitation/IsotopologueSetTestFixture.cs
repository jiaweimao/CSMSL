﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Should.Fluent;
using CSMSL.Analysis.Quantitation;
using CSMSL.Proteomics;
using CSMSL.Chemistry;

namespace CSMSL.Tests.Analysis.Quantitation
{
    [TestFixture]
    [Category("Peptide")]
    public sealed class IsotopologueSetTestFixture
    {
        private QuantitationChannelSet _lysine6plex; 
        
        [SetUp]
        public void SetUp()
        {
            _lysine6plex = new QuantitationChannelSet("Lysine 6-plex");
            _lysine6plex.Add(new Isotopologue("C-6 C{13}6 N-2 N{15}2", "C6 N2"));
            _lysine6plex.Add(new Isotopologue("C-4 C{13}4 N-2 N{15}2 H-2 H{2}2", "C4 N2 D2"));
            _lysine6plex.Add(new Isotopologue("C-5 C{13}5 N-1 N{15}1 H-2 H{2}2", "C5 N1 D2"));
            _lysine6plex.Add(new Isotopologue("C-3 C{13}3 N-1 N{15}1 H-4 H{2}4", "C3 N1 H4"));
            _lysine6plex.Add(new Isotopologue("C-4 C{13}4 H-4 H{2}4", "C4 D4"));
            _lysine6plex.Add(new Isotopologue("H-8 H{2}8", "D8"));
        }

        [Test]
        public void IsotopologueCount()
        {
            _lysine6plex.Count.Should().Equal(6);
        }

        [Test]
        public void IsotopologueMonoisotopicMass()
        {
            _lysine6plex.AverageMass.Monoisotopic.Should().Equal(8.0302584511);
        }

        [Test]
        public void IsotopologuePeptideMass()
        {
            Peptide pep = new Peptide("DEREK");
            pep.SetModification(_lysine6plex, 3);

            pep.Mass.Monoisotopic.Should().Equal(683.34902637217);
        }

        [Test]
        public void IsotopologuePeptideFragmentation()
        {
            Peptide pep = new Peptide("DEREK");
            pep.SetModification(_lysine6plex, 3);
            pep.CalculateFragments(FragmentTypes.b | FragmentTypes.y);
            pep.Mass.Monoisotopic.Should().Equal(683.34902637217);
        }        
    }
}