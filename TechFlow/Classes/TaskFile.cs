using System;

namespace TechFlow.Classes
{
    public class TaskFile
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public DateTime CreationDate { get; set; }
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }

        public TaskFile(int fileId, string fileName, string filePath, long fileSize,
                       string fileType, DateTime creationDate, int taskId, int employeeId)
        {
            FileId = fileId;
            FileName = fileName;
            FilePath = filePath;
            FileSize = fileSize;
            FileType = fileType;
            CreationDate = creationDate;
            TaskId = taskId;
            EmployeeId = employeeId;
        }

        public string FileSizeFormatted
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = FileSize;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}
