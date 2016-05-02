﻿using MeSHService.Model;
using MeSHService.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeSHService.Helpers.StringExtensions;

namespace MeSHService.Service
{
    /// <summary>
    /// Provides interface for main search engine module
    /// </summary>
    public class MeshService
    {
        MeshDictionary _dictionary;
        
        public MeshService(StringTransformation unifyingAlgorithm)
        {
            var dictBuilder = new MeshDictionaryBuilder();
            DescriptorRecordSet xmlResult = XmlMeshReader.Read();
            _dictionary = dictBuilder.Build(xmlResult, unifyingAlgorithm);
        }

        public IList<string> GetSynonyms(string unifiedTerm)
        {
            MeshDescriptor matchingDescriptor = null;

            if (_dictionary.DescriptorsByTerms.ContainsKey(unifiedTerm))
            {
                matchingDescriptor = _dictionary.DescriptorsByTerms[unifiedTerm];
            }
            else
            {
                matchingDescriptor = _dictionary.DescriptorsByTerms.FirstOrDefault(
                    desc =>
                        desc.Key.Contains(unifiedTerm)).Value;
            }

            return matchingDescriptor != null
                ? matchingDescriptor.Terms
                    .Where(t =>
                        !t.IsPermutedTerm)
                    .Select(t =>
                        t.TextValue)
                    .ToList()
                : new List<string> { unifiedTerm };
        }
    }
}
