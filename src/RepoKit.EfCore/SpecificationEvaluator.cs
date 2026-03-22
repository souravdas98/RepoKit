using AutoRepository.Core;
using Microsoft.EntityFrameworkCore;

namespace AutoRepository.EfCore;

/// <summary>
/// Translates <see cref="ISpecification{T}"/> instances into Entity Framework Core LINQ queryables.
/// This is used internally by <see cref="EfRepository{T}"/> to apply filtering, ordering, eager loading, and paging.
/// </summary>
internal static class SpecificationEvaluator
{
    /// <summary>
    /// Applies a specification to an IQueryable, building a query with filtering, eager loading, ordering, and paging.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="inputQuery">The input queryable (typically DbSet.AsQueryable()).</param>
    /// <param name="specification">The specification containing query criteria and configuration.</param>
    /// <returns>A new queryable with the specification applied.</returns>
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        ISpecification<T> specification
    )
        where T : class
    {
        var query = inputQuery;

        // Apply WHERE criteria
        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        // Apply includes
        query = specification.Includes.Aggregate(query, (q, i) => q.Include(i));
        query = specification.IncludeStrings.Aggregate(query, (q, i) => q.Include(i));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            var ordered = query.OrderBy(specification.OrderBy);
            query = ApplyThenBys(ordered, specification);
        }
        else if (specification.OrderByDescending != null)
        {
            var ordered = query.OrderByDescending(specification.OrderByDescending);
            query = ApplyThenBys(ordered, specification);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
            query = query.Skip(specification.Skip).Take(specification.Take);

        return query;
    }

    /// <summary>
    /// Applies secondary ordering (THEN BY) clauses to an ordered queryable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="ordered">The already-ordered queryable.</param>
    /// <param name="specification">The specification containing ThenBy clauses.</param>
    /// <returns>The queryable with ThenBy clauses applied.</returns>
    private static IQueryable<T> ApplyThenBys<T>(
        IOrderedQueryable<T> ordered,
        ISpecification<T> specification
    )
        where T : class
    {
        foreach (var (keySelector, descending) in specification.ThenBys)
            ordered = descending
                ? ordered.ThenByDescending(keySelector)
                : ordered.ThenBy(keySelector);

        return ordered;
    }
}
