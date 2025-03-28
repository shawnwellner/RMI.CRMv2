using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRM.Domain;
using Newtonsoft.Json;
using CRM.Models;

namespace CRM {
    public class WebSockets : Hub {
        private static object syncObj = new object();
        private static Dictionary<int, List<string>> _clientIds = new Dictionary<int, List<string>>();
        
        public void currentUser(string id, string currentUser) {
            //Clients.All.broadcastMessage(string.Format("{{ \"id\":\"{0}\", \"message\":\"{1}\" }}", id, message));
            lock(syncObj) {
                dynamic user = JsonConvert.DeserializeObject(currentUser);
                int userId = user.ClientParentId > 0 ? user.ClientParentId : user.UserId;
                if (_clientIds.ContainsKey(userId)) {
                    _clientIds[userId].Add(id);
                } else {
                    List<string> list = new List<string>();
                    list.Add(id);
                    _clientIds.Add(userId, list);
                }
            }
        }

        public void setTransfered(CustomerTransferModel model, bool transfered) {
            lock (syncObj) {
                if (_clientIds.ContainsKey(model.ClientId)) {
                    Clients.Clients(_clientIds[model.ClientId]).onTransferedChange(model, transfered);
                }
            }
        }

        public static void BroadcastTransferedChange(CustomerTransferModel model, bool transfered) {
            lock (syncObj) {
                var context = GlobalHost.ConnectionManager.GetHubContext<WebSockets>();
                if (_clientIds.ContainsKey(model.ClientId)) {
                    context.Clients.Clients(_clientIds[model.ClientId]).onTransferedChange(model, transfered);
                }
            }
        }

        public override System.Threading.Tasks.Task OnConnected() {
            lock (syncObj) {
                string connId = this.Context.ConnectionId;
                Clients.Client(connId).onConnected(connId);
                return base.OnConnected();
            }
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled) {
            lock (syncObj) {
                string connId = this.Context.ConnectionId;
                foreach (int key in _clientIds.Keys) {
                    _clientIds[key].Remove(connId);
                }
                return base.OnDisconnected(stopCalled);
            }
        }
    }
}