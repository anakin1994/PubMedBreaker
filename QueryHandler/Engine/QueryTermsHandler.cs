﻿using MeSHService.Service;
using QueryHandler.TextUnifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeSHService.Helpers.StringExtensions;

namespace QueryHandler.Engine
{
    public class QueryTermsHandler
    {
        public StringTransformation UnifyTerm;

        private MeshService service;      
        private IDictionary<string, List<Term>> globalDividedQueriesDict = new Dictionary<string, List<Term>>();

        public QueryTermsHandler(StringTransformation unifyFunc)
        {
            UnifyTerm = unifyFunc;
            service = new MeshService(unifyFunc);
        }

        public List<string> GetSynonyms(string query)
        {
            List<Term> termsWithSynonyms = GetDividedToTerms(query);
            return (termsWithSynonyms.Select(t => t.Synonyms)).SelectMany(x => x).ToList();
        }

        public List<Term> GetDividedToTerms(string query)
        {
            if (!globalDividedQueriesDict.ContainsKey(query))
                globalDividedQueriesDict.Add(query, DivideToTermsWithSynonyms(query));
            return globalDividedQueriesDict[query];
        }

        private List<Term> DivideToTermsWithSynonyms(string termsStr)
        {
            var result = new List<Term>();
            termsStr = UnifyTerm(termsStr);
            var words = termsStr.Split().ToList();
            int longestTermToMatchLength = words.Count > 20 ? 20 : words.Count;
            for (int termLength = longestTermToMatchLength; termLength > 1; termLength--)
            {
                for (int i = 0; i + termLength <= words.Count; i++)
                {
                    string currentlyChecked = words.JoinRange(i, termLength);
                    IList<string> synonyms = service.GetExactSynonyms(currentlyChecked);
                    if (synonyms.Any())
                    {
                        if (i > 0)
                            result = DivideToTermsWithSynonyms(words.JoinRange(0, i));

                        result.Add(new Term(currentlyChecked,synonyms));

                        if (i + termLength < words.Count)
                            result.AddRange(DivideToTermsWithSynonyms(words.JoinRange(i + termLength)));

                        return result;
                    }
                }
            }

            foreach (string word in words)
            {
                string unified = UnifyTerm(word);
                if(!string.IsNullOrWhiteSpace(unified))
                    result.Add(new Term(unified, service.GetSynonyms(unified)));
            }
            
            return result;
        }

    }
}
