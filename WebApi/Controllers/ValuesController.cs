using System.Linq;
using System.Net;
using System.Web.Http;
using System.IO;
using System.Web.ModelBinding;

namespace WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        const int SMALL_SIZE = 10 * 1024 * 1024;
        const int MEDIUM_SIZE = 50 * 1024 * 1024;
        const int LARGE_SIZE = 100 * 1024 * 1024;

        public Models.Directory Get(string path)
        {
            if (path == null)
            {
                return SeeDrives();
            }
            var directory = SeeDir(path);
            if (directory == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            return directory;
        }
        
        private void SeePath([QueryString("path")] string path)
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                SeeDir(path);
            }
            else
            {
                SeeFile(path);
            }
        }

        private void SeeFile(string path)
        {
        }

        private Models.Directory SeeDrives()
        {
            var drives = from drive in DriveInfo.GetDrives()
                         select new Models.DirectoryItem
                         {
                             Name = drive.Name,
                             Path = drive.Name
                         };
            var directory = new Models.Directory
            {
                Path = "",
                Directories = drives.ToList()
            };
            return directory;
        }

        private Models.Directory SeeDir(string path)
        {

            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
                return null;
            var files = dirInfo.GetFiles();
            var dirs = dirInfo.GetDirectories();
            var counts = files.GroupBy(f =>
            {
                return f.Length < SMALL_SIZE ? 0 :
                    f.Length < MEDIUM_SIZE ? 1 :
                    f.Length > LARGE_SIZE ? 2 :
                    3;
            })
            .Select(g => g.Count())
            .ToList();

            var directory = new Models.Directory();

            directory.Files = (from file in files
                               select new Models.DirectoryItem
                               {
                                   Name = file.Name,
                                   Path = file.FullName
                               }).ToList();
            directory.Directories = (from dir in dirs
                                     select new Models.DirectoryItem
                                     {
                                         Name = dir.Name,
                                         Path = dir.FullName
                                     }).ToList();
            directory.Directories.Insert(0, new Models.DirectoryItem
            {
                Name = "..",
                Path = dirInfo.Parent?.FullName
            });

            directory.Path = path;

            if (counts.Count > 0)
                directory.SmallFilesCount = counts[0];
            if (counts.Count > 1)
                directory.MediumFilesCount = counts[1];
            if (counts.Count > 2)
                directory.LargeFilesCount = counts[2];

            return directory;
        }

    }
}
