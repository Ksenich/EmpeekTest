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

        enum FileSize
        {
            SMALL,
            MEDIUM,
            LARGE,
            LARGEST
        }

        public Models.Directory Get([QueryString("path")] string path)
        {
            if (path == null || path == "")
            {
                return SeeDrives();
            }
            var directory = SeeDir(path);
            if (directory == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            return directory;
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
                return f.Length < SMALL_SIZE ? FileSize.SMALL :
                    f.Length < MEDIUM_SIZE ? FileSize.MEDIUM :
                    f.Length < LARGE_SIZE ? FileSize.LARGE :
                    FileSize.LARGEST;
            })
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            );

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

            if (counts.ContainsKey(FileSize.SMALL))
                directory.SmallFilesCount = counts[FileSize.SMALL];
            if (counts.ContainsKey(FileSize.MEDIUM))
                directory.MediumFilesCount = counts[FileSize.MEDIUM];
            if (counts.ContainsKey(FileSize.LARGEST))
                directory.LargeFilesCount = counts[FileSize.LARGEST];

            return directory;
        }

    }
}
