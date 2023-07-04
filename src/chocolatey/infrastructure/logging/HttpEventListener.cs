// Copyright © 2017 - 2023 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.logging
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Text;

    // Based on https://github.com/dotnet/runtime/issues/64977
    internal sealed class HttpEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Allow internal HTTP logging
            if (eventSource.Name.StartsWith("Private.InternalDiagnostics.System.Net."))
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // Log whatever other properties you want, this is just an example
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}] ");
            for (int i = 0; i < eventData.Payload?.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
            }
            try
            {
                Console.WriteLine(sb.ToString());
            }
            catch { }
        }
    }
}
