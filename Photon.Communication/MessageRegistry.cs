﻿using Photon.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Photon.Communication
{
    internal delegate Task<IResponseMessage> ProcessEvent(MessageTransceiver transceiver, IRequestMessage message);

    public class MessageRegistry
    {
        private readonly Dictionary<Type, ProcessEvent> processorMap;


        public MessageRegistry()
        {
            processorMap = new Dictionary<Type, ProcessEvent>();
        }

        public void Scan(Assembly assembly)
        {
            var classTypes = assembly.DefinedTypes
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var classType in classTypes)
                Register(classType);
        }

        internal async Task<IResponseMessage> Process(MessageTransceiver transceiver, IRequestMessage requestMessage)
        {
            var requestType = requestMessage.GetType();

            if (!processorMap.TryGetValue(requestType, out var processorFunc))
                throw new Exception($"No processor found matching request type '{requestType.Name}'!");

            return await processorFunc.Invoke(transceiver, requestMessage);
        }

        public void Register<TProcessor, TRequest>()
            where TProcessor : IProcessMessage<TRequest>
            where TRequest : IRequestMessage
        {
            Register(typeof(TProcessor));
        }

        public void Register(Type processorClassType)
        {
            var typeGenericMessageProcessor = typeof(IProcessMessage<>);

            foreach (var classInterface in processorClassType.GetInterfaces()) {
                if (!classInterface.IsGenericType) continue;

                var classInterfaceGenericType = classInterface.GetGenericTypeDefinition();
                if (classInterfaceGenericType != typeGenericMessageProcessor) continue;

                var argumentTypeList = classInterface.GetGenericArguments();
                if (argumentTypeList.Length != 1) continue;

                var requestType = argumentTypeList[0];

                var method = classInterface.GetMethod("Process");
                if (method == null) continue;

                processorMap[requestType] = (transceiver, message) =>
                    OnProcess(transceiver, processorClassType, method, message);
            }
        }

        private static async Task<IResponseMessage> OnProcess(MessageTransceiver transceiver, Type processorClassType, MethodInfo processMethod, IRequestMessage request)
        {
            object processor = null;

            try {
                processor = Activator.CreateInstance(processorClassType);

                processorClassType.GetProperty("Transceiver")?.SetValue(processor, transceiver);
                //processorClassType.GetProperty("Context")?.SetValue(processor, context);

                var arguments = new object[] {request};

                var result = processMethod.Invoke(processor, arguments);

                return await (Task<IResponseMessage>)result;
            }
            finally {
                (processor as IDisposable)?.Dispose();
            }
        }
    }
}
