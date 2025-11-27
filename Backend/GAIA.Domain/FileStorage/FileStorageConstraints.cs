namespace GAIA.Domain.FileStorage;

public static class FileStorageConstraints
{
  /// <summary>
  /// Matches the 255 character file-name limit enforced by NTFS/ext4 so uploaded files remain portable across platforms.
  /// </summary>
  public const int MaxFileNameLength = 255;
}
