namespace Nexus.Catalog.Domain;

/// <summary>
/// Entidade que representa uma categoria de produtos no catálogo.
/// Suporta hierarquia através de ParentCategoryId, permitindo árvores de categorias
/// (ex: Eletrônicos > Celulares > Acessórios). Cada categoria pode ser ativada/desativada
/// para controlar sua visibilidade na loja.
/// </summary>
public sealed class Category
{
    // Identificador textual (string) para facilitar legibilidade em URLs e APIs
    public string Id { get; private set; }

    // Nome da categoria — limite de 100 caracteres
    public string Name { get; private set; }

    // Descrição opcional — limite de 500 caracteres
    public string? Description { get; private set; }

    // Categoria pai (null se for categoria raiz). Cria hierarquia de navegação.
    public string? ParentCategoryId { get; private set; }

    // Ordem de exibição na interface (permite ordenação manual)
    public int DisplayOrder { get; private set; }

    // Define se a categoria está visível na loja (soft delete / controle de exibição)
    public bool IsActive { get; private set; }

    // Data de criação (imutável após definição)
    public DateTime CreatedAt { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Category() { }

    /// <summary>
    /// Cria uma nova categoria ativa com os dados fornecidos.
    /// O Id é gerado automaticamente como string UUID para compatibilidade com URLs amigáveis.
    /// </summary>
    public Category(string name, string? description = null, string? parentCategoryId = null, int displayOrder = 0)
    {
        Id = Guid.NewGuid().ToString();
        SetName(name);
        Description = description;
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera o nome da categoria com validação de tamanho.
    /// </summary>
    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        if (name.Length > 100) throw new ArgumentException("Category name must not exceed 100 characters", nameof(name));
        Name = name;
    }

    /// <summary>
    /// Altera a descrição com validação de tamanho máximo.
    /// Define como null para remover a descrição atual.
    /// </summary>
    public void SetDescription(string? description)
    {
        if (description?.Length > 500) throw new ArgumentException("Description must not exceed 500 characters", nameof(description));
        Description = description;
    }

    /// <summary>
    /// Altera a ordem de exibição da categoria (quanto menor, mais acima aparece).
    /// </summary>
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }

    /// <summary>
    /// Desativa a categoria (não é mais exibida, mas permanece no banco).
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Reativa a categoria.
    /// </summary>
    public void Activate() => IsActive = true;
}