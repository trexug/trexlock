using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TrexLetsDuck
{
	public interface ICertificateStorage
	{
		public string Key { get; set; }
		public X509Certificate2 Certificate { get; }
		public Task UpdateCertificateAsync(byte[] bytes);
	}
}
