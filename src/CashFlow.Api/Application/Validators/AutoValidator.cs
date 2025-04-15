using System.Linq.Expressions;

namespace CashFlow.Api.Application.Validators;

public class AutoValidator
{
    public AutoValidator<T> Build<T>() where T : class
        => new AutoValidator<T>();
}

public class AutoValidator<T>
{
    private readonly IList<Func<T, Task<(bool IsValid, string? Field, string? ErrorMessage)>>> _validators;

    public AutoValidator()
    {
        _validators = [];
    }

    public AutoValidator<T> With<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        Func<TProperty, Task<bool>> validator,
        string? errorMessage = null)
    {
        var propertyName = ((MemberExpression)propertySelector.Body).Member.Name;

        _validators.Add(async obj =>
        {
            var compiledSelector = propertySelector.Compile();
            var value = compiledSelector(obj);
            var result = await validator(value);
            return (result, propertyName, result ? null : errorMessage ?? $"{propertyName} is invalid.");
        });

        return this;
    }

    public AutoValidator<T> With<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        Func<TProperty, bool> validator,
        string? errorMessage = null)
    {
        return With(propertySelector, value => Task.FromResult(validator(value)), errorMessage);
    }

    public async Task<(bool IsValid, Dictionary<string, string> Errors)> ValidateAsync(T instance)
    {
        var errors = new Dictionary<string, string>();

        foreach (var validator in _validators)
        {
            var (isValid, field, message) = await validator(instance);

            if (!isValid && field is not null && message is not null)
            {
                if (errors.ContainsKey(field))
                    errors[field] = $"{errors[field]}{Environment.NewLine}{message}";

                errors[field] = message;
            }
        }

        return (errors.Count == 0, errors);
    }
}

public static class AutoValidatorExtension
{
    public static async Task<bool> ValidateAsync<T>(this AutoValidator<T> autoValidator, T instance)
    {
        var (isValid, _) = await autoValidator.ValidateAsync(instance);
        return isValid;
    }
}