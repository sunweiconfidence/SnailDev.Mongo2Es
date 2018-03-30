﻿using Elasticsearch.Net;
using Mongo2Es.Log;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mongo2Es.ElasticSearch
{
    public class EsClient
    {
        private ElasticLowLevelClient client;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="url"></param>
        public EsClient(string url)
        {
            var settings = new ConnectionConfiguration(new Uri(url)).RequestTimeout(TimeSpan.FromSeconds(10));
            this.client = new ElasticLowLevelClient(settings);
        }

        /// <summary>
        /// 插入文档
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="doc"></param>
        public bool InsertDocument(string index, string type, string id, BsonDocument doc)
        {
            bool flag = false;

            try
            {
                var resStr = client.Index<StringResponse>(index, type, id, PostData.String(doc.ToJson()));
                var resObj = JObject.Parse(resStr.Body);
                if ((int)resObj["_shards"]["successful"] > 0)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(logger, ex.ToString(), id);
            }

            return flag;
        }

        /// <summary>
        /// 批量插入文档
        /// </summary>
        /// <param name="docs"></param>
        /// <returns></returns>
        public bool InsertBatchDocument(IEnumerable<string> docs)
        {
            bool flag = false;

            try
            {
                var resStr = client.Bulk<StringResponse>(PostData.MultiJson(docs));
                var resObj = JObject.Parse(resStr.Body);
                if (!(bool)resObj["errors"])
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return flag;
        }

        /// <summary>
        /// 批量插入文档
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="docs"></param>
        /// <returns></returns>
        public bool InsertBatchDocument(string index, string type, List<string> docs)
        {
            bool flag = false;
            if (docs.Count < 1)
            {
                flag = true;
                return flag;
            }

            try
            {
                var resStr = client.Bulk<StringResponse>(index, type, PostData.MultiJson(docs));
                var resObj = JObject.Parse(resStr.Body);
                if (!(bool)resObj["errors"])
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return flag;
        }


        /// <summary>
        /// 更新文档
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="doc"></param>
        public bool UpdateDocument(string index, string type, string id, BsonDocument doc)
        {
            bool flag = false;
            if (doc.Names.Count() < 1)
            {
                flag = true;
                return flag;
            }
            
            try
            {
                var resStr = client.Update<StringResponse>(index, type, id, PostData.String(BsonDocument.Parse($"{{'doc':{doc}}}").ToJson()));
                var resObj = JObject.Parse(resStr.Body);
                if ((int)resObj["_shards"]["total"] == 0 || (int)resObj["_shards"]["successful"] > 0)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(logger, ex.ToString(), id);
            }

            return flag;
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public bool DeleteDocument(string index, string type, string id)
        {
            bool flag = false;

            try
            {
                var resStr = client.Delete<StringResponse>(index, type, id);
                var resObj = JObject.Parse(resStr.Body);
                if ((int)resObj["_shards"]["total"] == 0 || (int)resObj["_shards"]["successful"] > 0)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(logger, ex.ToString(), id);
            }

            return flag;
        }
    }
}
