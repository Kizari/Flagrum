﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Application.Persistence.Entities.ModManager;

public class EarcModEarc
{
    public int Id { get; set; }

    public int EarcModId { get; set; }
    public EarcMod EarcMod { get; set; }

    public string EarcRelativePath { get; set; }
    public ModChangeType Type { get; set; }

    /// <summary>
    /// Only applies to Add type
    /// </summary>
    public EbonyArchiveFlags Flags { get; set; }

    public ICollection<EarcModFile> Files { get; set; } = new List<EarcModFile>();

    [NotMapped] public bool IsExpanded { get; set; }
}