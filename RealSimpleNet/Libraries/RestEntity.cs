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

        public event RestOperationSuccessHandler OnRestfullSuccess;
        public event RestOperationErrorHandler OnRestfullError;

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
                OnRestfullSuccess(rest.Post(this.GetType().Name, this));
            } catch(Exception ex)
            {
                OnRestfullError(ex);
            } // end function            
        } // end function ExecPost

        /// <summary>
        /// Makes a "PUT" call to the api 
        /// </summary>
        private void ExecPut()
        {
            try
            {
                OnRestfullSuccess(rest.Put(this.GetType().Name, this));
            }
            catch (Exception ex)
            {
                OnRestfullError(ex);
            } // end function     
        } // end function ExecPost

        /// <summary>
        /// Makes a "DELETE" call to the api 
        /// </summary>
        private void ExecDelete()
        {
            try
            {
                OnRestfullSuccess(rest.Delete(this.GetType().Name, this));
            }
            catch (Exception ex)
            {
                OnRestfullError(ex);
            } // end function     
        } // end function ExecPost

        /// <summary>
        /// Validates variable endpoint present
        /// </summary>
        private void Validate()
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                throw new Exception("Endpoint not present");
            } // end if
            
            if (OnRestfullSuccess == null)
            {
                throw new Exception("Success handler not present");
            }

            if (OnRestfullError == null)
            {
                throw new Exception("Error handler not present");
            }
        } // end function Validate
        
        /// <summary>
        /// Start a new thread for a post call
        /// </summary>
        public void RestfulPost()
        {
            Validate();
            Thread t = new Thread(ExecPost);
            t.Start();
        } // end function post

        /// <summary>
        /// Makes a "POST" call to the api
        /// </summary>
        public void RestfulPut()
        {
            Validate();
            Thread t = new Thread(ExecPut);
            t.Start();
        } // end function post

        /// <summary>
        /// Makes a "PUT" call to the api
        /// </summary>
        public void RestfulDelete()
        {
            Validate();
            Thread t = new Thread(ExecDelete);
            t.Start();
        } // end function post
    } // end class RestEntity
}
