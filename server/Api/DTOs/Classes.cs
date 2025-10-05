namespace Api.DTOs;

public record ClassResponse(string Id, string Name);
public record ClassCreate(string Id, string Name);
public record ClassUpdate(string Name);
