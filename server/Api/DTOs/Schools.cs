namespace Api.DTOs;

public record SchoolResponse(string Id, string Name);
public record SchoolCreate(string Id, string Name);
public record SchoolUpdate(string Name);