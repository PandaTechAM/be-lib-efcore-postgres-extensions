namespace EFCore.PostgresExtensions.Helpers;

/// <summary>
///    Provides SQL generation helpers for PostgreSQL functions used by this package.
/// </summary>
internal static class PgFunctionHelpers
{
   /// <summary>
   ///    Builds SQL that creates a random-ID sequence and generator function for a table primary key.
   /// </summary>
   /// <param name="tableName">The table that owns the generated primary-key values.</param>
   /// <param name="pkName">The primary-key column name used to derive the sequence name.</param>
   /// <param name="startValue">The initial sequence value.</param>
   /// <param name="minRandIncrementValue">The minimum random increment applied after each generated value.</param>
   /// <param name="maxRandIncrementValue">The maximum random increment applied after each generated value.</param>
   /// <returns>The SQL script that creates the sequence and generator function.</returns>
   public static string GetRandomIdFunctionSql(string tableName,
      string pkName,
      long startValue,
      int minRandIncrementValue,
      int maxRandIncrementValue)
   {
      var sequenceName = GetSequenceName(tableName, pkName);
      var pgFunctionName = GetRandomIdFunctionName(tableName);

      return $"""
                  -- Create sequence if not exists
              DO $$
              BEGIN
                  IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relkind = 'S' AND relname = '{sequenceName}') THEN
                      CREATE SEQUENCE {sequenceName} START WITH {startValue};
                  END IF;
              END
              $$;

                  -- Create or replace the function
                  CREATE OR REPLACE FUNCTION {pgFunctionName}
                  RETURNS bigint AS $$
                  DECLARE
                      current_value bigint;
                      increment_value integer;
                      new_value bigint;
                  BEGIN
                      -- Acquire an advisory lock
                      PERFORM pg_advisory_lock(1);

                      -- Get the next value of the sequence atomically
                      current_value := nextval('{sequenceName}'); -- name of the sequence
                      
                      -- Generate a random increment between {minRandIncrementValue} and {maxRandIncrementValue}
                      increment_value := floor(random() * ({maxRandIncrementValue} - {minRandIncrementValue} + 1) + {minRandIncrementValue})::integer;
                      
                      -- Set the new value with the random increment
                      new_value := current_value + increment_value;
                      
                      -- Update the sequence to the new value
                      PERFORM setval('{sequenceName}', new_value, true); -- name of the sequence
                      
                      -- Release the advisory lock
                      PERFORM pg_advisory_unlock(1);

                      RETURN new_value;
                  END;
                  $$ LANGUAGE plpgsql;
              """;
   }

   private static string GetSequenceName(string tableName, string pkName)
   {
      return $"{tableName}_{pkName}_seq";
   }

   /// <summary>
   ///    Gets the PostgreSQL random-ID generator function name for the specified table.
   /// </summary>
   /// <param name="tableName">The table that owns the generator function.</param>
   /// <returns>The PostgreSQL function invocation name.</returns>
   public static string GetRandomIdFunctionName(string tableName)
   {
      return $"{tableName}_random_id_generator()";
   }

   /// <summary>
   ///    Builds SQL that creates or replaces the natural sort key function.
   /// </summary>
   /// <returns>The SQL script that creates the natural sort key function.</returns>
   public static string GetNaturalSortKeyFunction()
   {
      return """
             CREATE OR REPLACE FUNCTION get_natural_sort_key(input_text TEXT)
             RETURNS TEXT
             LANGUAGE sql
             IMMUTABLE
             AS $$
               WITH tokens AS (
                 -- This splits the string into digit blocks (\d+) or non-digit blocks (\D+).
                 SELECT regexp_matches(input_text, '(\d+|\D+)', 'g') AS parts
               ),
               padded AS (
                 SELECT (
                   CASE
                     WHEN parts[1] ~ '^\d+$'
                     THEN LPAD(parts[1], 10, '0')  -- Zero-pad numeric tokens to length 10
                     ELSE parts[1]                -- Leave everything else as-is
                   END
                 ) AS chunk
                 FROM tokens
               )
               SELECT string_agg(chunk, '')
               FROM padded;
             $$;
             """;
   }
}
