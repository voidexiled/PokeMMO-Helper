#!/bin/bash
# Fix PokemonService.cs - remove cache from inside method and add at class level

cd "$(dirname "$0")"

# Delete the incorrect line 21
sed -i '21d' services/PokemonService.cs

# Add cache declaration after line 17 (now line 16 after deletion)
sed -i '17 a\    private static readonly Dictionary<string, PokemonMove> _moveCache = new Dictionary<string, PokemonMove>();' services/PokemonService.cs

echo "✅ Cache moved to class level!"
