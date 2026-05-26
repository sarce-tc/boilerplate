# /arq-controller — Patrón controller

Leer: `API/Controllers/OrdersController.cs`

Variantes de binding:
- **A** solo ruta → `new XCommand(publicId)`
- **B** ruta + body → `command with { PublicId = publicId }`
- **C** 201 Created → `result.ToActionResult(StatusCodes.Status201Created)`
