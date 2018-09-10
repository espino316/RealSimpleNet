using RealSimpleNet.Helpers;
using System.Threading;
using System;

namespace RealSimpleNet.Libraries
{
    public class RestEntity
    {
        public string Endpoint = "";

        public delegate void RestOperationSuccessHandler(string response);
        public delegate void RestOperationErrorHandler(Exception ex);

        public event RestOperationSuccessHandler OnSuccess;
        public event RestOperationErrorHandler OnError;

        private RestClient rest;

        public RestEntity()
        {
        }

        public void SetEndpoint(string endpoint)
        {
            Endpoint = endpoint;
            rest = new RestClient(Endpoint);
        }

        /// <summary>
        /// Makes a "POST" call to the api 
        /// </summary>
        private void ExecPost()
        {
            try
            {
                OnSuccess(rest.Post(this.GetType().Name, this));
            } catch(Exception ex)
            {
                OnError(ex);
            } // end function            
        } // end function ExecPost

        /// <summary>
        /// Makes a "PUT" call to the api 
        /// </summary>
        private void ExecPut()
        {
            try
            {
                OnSuccess(rest.Put(this.GetType().Name, this));
            }
            catch (Exception ex)
            {
                OnError(ex);
            } // end function     
        } // end function ExecPost

        /// <summary>
        /// Makes a "DELETE" call to the api 
        /// </summary>
        private void ExecDelete()
        {
            try
            {
                OnSuccess(rest.Delete(this.GetType().Name, this));
            }
            catch (Exception ex)
            {
                OnError(ex);
            } // end function     
        } // end function ExecPost

        /// <summary>
        /// Start a new thread for a post call
        /// </summary>
        public void Post()
        {
            ExecPost();return;
            Thread t = new Thread(ExecPost);
            t.Start();
        } // end function post

        /// <summary>
        /// Makes a "POST" call to the api
        /// </summary>
        public void Put()
        {
            Thread t = new Thread(ExecPut);
            t.Start();
        } // end function post

        /// <summary>
        /// Makes a "PUT" call to the api
        /// </summary>
        public void DeleteEntity()
        {
            Thread t = new Thread(ExecDelete);
            t.Start();
        } // end function post
    } // end class RestEntity
}
