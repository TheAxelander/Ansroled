using Ansroled.Models;

namespace Ansroled.Common.DialogResponses;

public record DeleteFileResponse(AnsibleFile File, Editor Editor);
