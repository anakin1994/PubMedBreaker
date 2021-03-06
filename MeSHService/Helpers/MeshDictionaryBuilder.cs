﻿using MeSHService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeSHService.Helpers.StringExtensions;

namespace MeSHService
{
    /// <summary>
    /// Builds MeSH dictionary from xml auto-generated classes objects
    /// </summary>
    public class MeshDictionaryBuilder
    { 
        private MeshDictionary _dictionary;

        public MeshDictionary Build(DescriptorRecordSet xmlTree, StringTransformation termStemmingAlgorithm)
        {
            _dictionary = new MeshDictionary(termStemmingAlgorithm);

            foreach (DescriptorRecord descriptor in xmlTree.DescriptorRecord)
                ProcessXmlDescriptor(descriptor);

            return _dictionary;
        }

        private void ProcessXmlDescriptor(DescriptorRecord record)
        {
            var desc = new MeshDescriptor();
            desc.Name = record.DescriptorName.String;

            if (record.TreeNumberList != null)
            {
                foreach (string treeNum in record.TreeNumberList.TreeNumber)
                    desc.TreeIds.Add(treeNum);
            }

            if (record.ConceptList != null)
            {
                foreach(Concept concept in record.ConceptList)
                {
                    if (concept.TermList != null)
                    {
                        foreach (Term term in concept.TermList)
                        {
                            var termModel = new MeshTerm
                            {
                                TextValue = term.String,
                                IsPermutedTerm = term.IsPermutedTermYN == TermIsPermutedTermYN.Y
                            };

                            desc.Terms.Add(termModel);
                        }
                    }
                }
            }

            _dictionary.Add(desc);
        }
    }
}
