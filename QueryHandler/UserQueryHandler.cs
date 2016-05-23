﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PubMedService;
using MeSHService.Service;
using System.Diagnostics;
using System.Linq;
using QueryHandler.TextUnifiers;
using QueryHandler.Engine;
using PubMedService.PubMedQueryBuilder;
using QueryHandler.Ranking;

namespace QueryHandler
{
    public class UserQueryHandler
    {
        private QueryTermsHandler termsHandler;
        private RankingBuilder rankingBuilder;

        public UserQueryHandler()
        {
            termsHandler = new QueryTermsHandler(Unification.Unify);
            rankingBuilder = new RankingBuilder(termsHandler);
        }
        
        public async Task<FinalResultsSet> GetResultsForQuery(string query, int resultsNumber, int timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var multiQueryBuilder = new QueriesSetBuilder(termsHandler, query, resultsNumber);
            IList<IPubMedQueryBuilder> queries = multiQueryBuilder.GetQueriesToSend();

            List<PubMedQueryResult> allResults = new List<PubMedQueryResult>();

            foreach (IPubMedQueryBuilder queryToBuild in queries)
            {
                List<PubMedQueryResult> queryResults =
                    await PubMedQueryHandler.GetResultsForPubMedQueryAsync(queryToBuild);
                allResults.AddRange(queryResults);
            }

            FinalResultsSet frs = new FinalResultsSet();
            frs.ExecutionTimeMilis = stopwatch.ElapsedMilliseconds;
            frs.UnifiedQuery = Unification.Unify(query);
            frs.Synonyms = termsHandler.GetSynonyms(query);

            IList<PubMedArticleResult> results = rankingBuilder.Build(query, allResults);
            frs.UserQueryResults = results.OrderByDescending(r => r.RankingVal).ToList();
            
            return frs;
        }
    }
}
