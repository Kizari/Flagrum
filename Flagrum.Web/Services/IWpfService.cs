using System;
using System.Threading.Tasks;

namespace Flagrum.Web.Services;

public interface IWpfService
{
    Task OpenFileDialogAsync(string filter, Action<string> onFileSelected);
    Version GetVersion();
}