# When passed (1) a style name and (2) a string of supported characters, this script
# converts the string to Slack emoji symbols of the requested style. Case is ignored.
# The requested character emojis must, of course, be available in Slack.
# Update SUPPORTED_CHARSETS as needed to match the emojis available in Slack.
#
# Run: ruby slack-text-converter.rb STYLE_NAME "STRING_OF_SUPPORTED_CHARS"
#        (Example: ruby slack-text-converter.rb cookie "Sprint planning")
#
# Tip: On macOS, append ` | pbcopy` to output directly to your clipboard!

LETTERS = *('a'..'z')
NUMBERS = *('0'..'9')
SPACE = ' '

SUPPORTED_CHARSETS = {
  cookie:
    {
      chars: [*LETTERS, '!', '?', '&', '•', SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        when '!' then ':cookie-exclaim:'
        when '?' then ':cookie-question:'
        when '&' then ':cookie-and:'
        when '•' then ':cookie-dot:'
        else ":cookie-#{char}:"
        end
      end
    },
  bluebox:
    {
      chars: [*LETTERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank: '
        when 'o' then ':alpha-0: '
        else ":alpha-#{char}: "
        end
      end
    },
  alphawhite:
    {
      chars: [*LETTERS, '!', '?', '#', '@', SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        when '!' then ':alphabet-white-exclamation:'
        when '?' then ':alphabet-white-question:'
        when '#' then ':alphabet-white-hash:'
        when '@' then ':alphabet-white-at:'
        else ":alphabet-white-#{char}:"
        end
      end
    },
  alphayellow:
    {
      chars: [*LETTERS, '!', '?', '#', '@', 'ñ', SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        when '!' then ':alphabet-yellow-exclamation:'
        when '?' then ':alphabet-yellow-question:'
        when '#' then ':alphabet-yellow-hash:'
        when '@' then ':alphabet-yellow-at:'
        when 'ñ' then ':alphabet-yellow-nñ:'
        else ":alphabet-yellow-#{char}:"
        end
      end
    },
  alphasnow:
    {
      chars: [*LETTERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        when 'i' then ':alpha_snow_i2:'
        else ":alpha_snow_#{char}:"
        end
      end
    },
  tiles:
    {
      chars: [*LETTERS, 'ñ', SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':letter_blank:'
        when 'ñ' then ':letterñ:'
        else ":letter_#{char}:"
        end
      end
    },
  pokemon:
    {
      chars: [*LETTERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        else ":pokemonfont-#{char}:"
        end
      end
    },
  merah:
    {
      chars: [*LETTERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        # The "merah" set is incomplete and inconsistently named.
        when 'b' then ':merahbb:'
        when 'd' then ':merahdd:'
        when 'e' then ':merahee:'
        when 'f' then ':ff:'
        when 'h' then ':merahhh:'
        when 'k' then ':merahkk:'
        when 'l' then ':merahll:'
        when 'q' then ':alpha-q:'
        when 'r' then ':merahrr:'
        when 's' then ':merahsss:'
        when 'u' then ':merahuuu:'
        when 'x' then ':alpha-x:'
        when 'y' then ':merahhyyy:'
        when 'z' then ':alpha-z:'
        else ":merah#{char}:"
        end
      end
    },
  magazine:
    {
      chars: [*LETTERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        else ":magazine_#{char}:"
        end
      end
    },
  custom:
    {
      chars: [*LETTERS, *NUMBERS, '?', '!', SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        when 'a' then ":m-a:"
        when 'b' then ":b4:"
        when 'c' then ":c2:"
        when 'd' then ":devo-d:"
        when 'e' then ":edge:"
        when 'f' then ":ff:"
        when 'g' then ":g-neon:"
        when 'h' then ":h:"
        when 'i' then ":info:"
        when 'j' then ":super-j:"
        when 'k' then ":m'kay:"
        when 'l' then ":labsslide-1:"
        when 'm' then ":m:"
        when 'n' then ":n64:"
        when 'o' then ":o:"
        when 'p' then ":p2:"
        when 'q' then ":qflash:"
        when 'r' then ":r:"
        when 's' then ":scon:"
        when 't' then ":kid-t:"
        when 'u' then ":m-u:"
        when 'v' then ":devo-v:"
        when 'w' then ":walphabet:"
        when 'x' then ":x:"
        when 'y' then ":y1:"
        when 'z' then ":zelle_onfire:"
        when '0' then ":0_bats:"
        when '1' then ":number-1-red:"
        when '2' then ":number2:"
        when '3' then ":number-3-flip:"
        when '4' then ":mana-4:"
        when '5' then ":round-red-5:"
        when '6' then ":mana-6:"
        when '7' then ":7-up:"
        when '8' then ":8flower:"
        when '9' then ":9lego:"
        when '!' then ":exclamation:"
        when '?' then ":question-icon:"
        else char # Should not be reached.
        end
      end
    },
  numbers:
    {
      chars: [*NUMBERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        else ":num#{char}:"
        end
      end
    },
  squarenumbers:
    {
      chars: [*NUMBERS, SPACE],
      converter: ->(char) do
        case char
        when SPACE then ':blank:'
        when '0' then ':zero:'
        when '1' then ':one:'
        when '2' then ':two:'
        when '3' then ':three:'
        when '4' then ':four:'
        when '5' then ':five:'
        when '6' then ':six:'
        when '7' then ':seven:'
        when '8' then ':eight:'
        else ':nine:'
        end
      end
    }
  }

supported_styles = SUPPORTED_CHARSETS.keys

if ARGV.count.zero? || ARGV.count > 2
  STDERR.puts "Pass in (1) a style name and (2) a string containing only supported characters for that style."
  STDERR.puts "Supported styles: #{supported_styles.join(', ')}"
  return
end

style = ARGV[0].downcase.to_sym

def style_chars(style, separator)
  SUPPORTED_CHARSETS[style][:chars]
    .map { |c| c == SPACE ? '(space)' : c }
    .join(separator)
end

is_supported_style = supported_styles.include? style

if ARGV.count == 1 && is_supported_style
  STDERR.puts "The \"#{style}\" style supports the following characters:"
  STDERR.puts style_chars(style, SPACE)
  return
end

unless is_supported_style
  STDERR.puts "\"#{style}\" is not a supported style."
  STDERR.puts "Supported styles: #{supported_styles.join(', ')}"
  return
end

lowered_chars = ARGV[1].downcase.chars

if lowered_chars.length.zero?
  STDERR.puts "You must enter text for conversion."
  return
end

is_supported_char = ->(c) { SUPPORTED_CHARSETS[style][:chars].include? c }

unless lowered_chars.all?(&is_supported_char)
  STDERR.puts "Only the following characters are supported for the \"#{style}\" style:"
  STDERR.puts style_chars(style, SPACE)
  return
end

style_converter = SUPPORTED_CHARSETS[style][:converter]
puts lowered_chars.map(&style_converter).join
