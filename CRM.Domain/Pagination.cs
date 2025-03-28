using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
	public class GenericPropety<T> where T : class, new() {
		public T Value { get; set; }

		public static implicit operator T(GenericPropety<T> value) {
			return value.Value;
		}

		public static implicit operator GenericPropety<T>(T value) {
			return new GenericPropety<T>() {
				Value = value
			};
		}
	}

	public class Pagination<T> : Pagination where T : class, new() {
		public List<T> Items { get; private set; }
		
		public Pagination(sp_CustomerSearch_Result result) {
			this.Items = new List<T>();
			this.AutoFill(result);
		}

		public Pagination PageInfo => new Pagination(this);

		public static implicit operator Pagination<T>(ObjectResult<sp_CustomerSearch_Result> results) {
			List<sp_CustomerSearch_Result> items = results.ToList();
			Pagination<T> pageInfo = new Pagination<T>(items.FirstOrDefault());
			foreach (sp_CustomerSearch_Result val in items) {
				pageInfo.Items.Add(val.CloneAs<T>());
			}
			return pageInfo;
		}

		public static implicit operator Pagination<T>(sp_CustomerSearch_Result results) {
			//StackTrace trace = new StackTrace();
			//MethodInfo method = (MethodInfo)trace.GetFrame(1).GetMethod();
			Pagination<T> pageInfo = new Pagination<T>(results);
			pageInfo.Items.Add(results.CloneAs<T>());
			return pageInfo;
		}

		public int RemoveAll(Predicate<T> filter) {
			return this.Items.RemoveAll(filter);
		}

		public List<T> Where(Func<T, bool> filter) {
			return this.Items.Where(filter).ToList();
		}

		public void Foreach(Action<T> action) {
			this.Items.ForEach(action);
		}
	}


	public class Pagination {
		public Pagination() {
            
        }

        public Pagination(Pagination pageInfo) {
			this.PageNum = pageInfo.PageNum;
			this.MaxRows = pageInfo.MaxRows;
			this.PageCount = pageInfo.PageCount;
			this.TotalRecords = pageInfo.TotalRecords;
        }

        public int? PageNum { get; set; }
        public int? MaxRows { get; set; }
        public int PageCount { get; internal set; }
        public int TotalRecords { get; internal set; }
    }
}
