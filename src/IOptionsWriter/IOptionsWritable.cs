using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace IOptionsWriter
{
    public interface IOptionsWritable<out T> : IOptionsSnapshot<T> where T : class, new()
    {
        Task Update(Action<T> applyChanges);
    }
}