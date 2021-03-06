﻿using PubMedService;
using QueryHandler.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryHandler.Ranking
{
    public class RankingBuilder
    {
        private QueryTermsHandler termsHandler;
        private Document query; 
        private IList<PubMedArticleResult> matchedArticlesRanking;
        private IList<RankedTerm> queryTermsRankedWithIDF;
        
        public RankingBuilder(QueryTermsHandler termsHandler)
        {
            this.termsHandler = termsHandler;
        }

        public IList<PubMedArticleResult> Build(string queryStr, IDictionary<PubMedQueryResult, int> matchedArticles,
            int resultsCount)
        {
            query = new Document(queryStr, termsHandler);
            matchedArticlesRanking = matchedArticles
                .Select(
                    art =>
                        new PubMedArticleResult(art.Key, art.Value, termsHandler))
                .ToList();

            queryTermsRankedWithIDF = ComputeInverseDocumentFrequencies();

            foreach (PubMedArticleResult article in matchedArticlesRanking)
                article.CosineMetric = Match(query, article);

            var minMetric = matchedArticlesRanking.Min(x => x.CosineMetric);
            var maxMetric = matchedArticlesRanking.Max(x => x.CosineMetric);

            foreach (var article in matchedArticlesRanking)
            {
                article.RankingVal = NormalizeRanking(article.CosineMetric, minMetric, maxMetric) +
                                     (resultsCount - article.PubMedPosition)*0.01;
            }

            minMetric = matchedArticlesRanking.Min(x => x.RankingVal);
            maxMetric = matchedArticlesRanking.Max(x => x.RankingVal);

            foreach (var article in matchedArticlesRanking)
            {
                article.RankingVal = NormalizeRanking(article.RankingVal, minMetric, maxMetric);
                if (article.FoundCount == 2)
                    article.RankingVal += 0.2 * Math.Min(article.RankingVal, 1 - article.RankingVal);
                else if(article.FoundCount == 3)
                    article.RankingVal += 0.3 * Math.Min(article.RankingVal, 1 - article.RankingVal);
            }
            
            return matchedArticlesRanking;
        }

        private IList<RankedTerm> ComputeInverseDocumentFrequencies()
        {
            var result = new List<RankedTerm>();

            foreach (ComparableTerm term in query.Terms)
            {
                if (result.Any(t => t.Term.Equals(term)))
                    continue;

                var docsList = matchedArticlesRanking.Select(art => art.Document).ToList();
                docsList.Add(query);
                
                double idf = ComputeInverseDocumentFrequency(term, docsList);
                if (idf == 0)
                    idf = 0.0001;

                result.Add(new RankedTerm { Term = term, InverseDocumentFrequency = idf });
            }

            return result;
        }
        
        private double ComputeInverseDocumentFrequency(ComparableTerm term, IList<Document> documents)
        {
            double appearances = documents.Count(d => d.GetCountOf(term) > 0);
            double totalCount = documents.Count;
            return Math.Log(totalCount / appearances, 2);
        }

        private double Match(Document query, PubMedArticleResult article)
        {
            double scalarRatio = queryTermsRankedWithIDF
                .Select(
                    t =>
                        query.GetTFxIDF(t) * article.Document.GetTFxIDF(t))
                .Sum();

            double distanceRatio = Math.Sqrt(
                queryTermsRankedWithIDF
                .Select(
                    t =>
                        Math.Pow((query.GetTFxIDF(t) - article.Document.GetTFxIDF(t)), 2))
                .Sum());
            return scalarRatio / distanceRatio;
        }

        private double NormalizeRanking(double rankVal, double minVal, double maxVal)
        {
            return (rankVal - minVal)/(maxVal - minVal);
        }
    }
}
