namespace Siniskalc.Core.Runtime
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="ForwardingLogger"/> of the class <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The class whose name the log lines show.</typeparam>
    internal sealed class ForwardingLogger<T>() : ForwardingLogger(CategoryOf(typeof(T))), ILogger<T>
    {
    }
}
