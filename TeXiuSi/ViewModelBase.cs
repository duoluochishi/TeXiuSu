using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi
{
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="callback">消息实例</param>
        protected void ReceiveMessage<TMessage>(Action<TMessage> callback) where TMessage : class
        {
            WeakReferenceMessenger.Default.Register<TMessage>(this,
                ((recipient, message) =>
                {
                    try
                    {

                            callback.Invoke(message);
                       
                    }
                    catch (Exception ex)
                    {
                        Log.Information(typeof(ViewModelBase).FullName, ex);
                    }
                }));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="message">消息实例</param>
        protected void SendMessage<TMessage>(TMessage message) where TMessage : class
        {
            WeakReferenceMessenger.Default.Send(message);
        }
    }
}
