# When passed (1) a style name and (2) a string of supported characters, this script
# converts the string to Slack emoji symbols of the requested style. Case is ignored.
# The requested character emojis must, of course, be available in Slack.
# Update SUPPORTED_CHARS as needed to match the emojis available in Slack.
#
# Run: ruby slack-text-converter.rb STYLE_NAME "STRING_OF_SUPPORTED_CHARS"
#        (Example: ruby slack-text-converter.rb cookie "Sprint planning")
#
# Tip: On macOS, append ` | pbcopy` to output directly to your clipboard!

LETTERS = *('a'..'z')
NUMBERS = *('0'..'9')
SPACE = ' '

SUPPORTED_CHARS = {
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

supported_styles = SUPPORTED_CHARS.keys

if ARGV.count != 2
  STDERR.puts "Pass in (1) a style name and (2) a string containing only supported characters for that style."
  STDERR.puts "Supported styles:  #{supported_styles.join('  ')}"
  return
end

style = ARGV[0].downcase.to_sym
lowered_chars = ARGV[1].downcase.chars

unless supported_styles.include? style
  STDERR.puts "\"#{style}\" is not a supported style."
  STDERR.puts "Supported styles:  #{supported_styles.join('  ')}"
  return
end

is_supported_char = ->(c) { SUPPORTED_CHARS[style][:chars].include? c }

unless lowered_chars.all?(&is_supported_char)
  STDERR.puts "Invalid characters found! Only the following characters are supported for the \"#{style}\" style:"
  style_chars = SUPPORTED_CHARS[style][:chars].map { |c| c == SPACE ? '(space)' : c }
  STDERR.puts style_chars.join(SPACE)
  return
end

style_converter = SUPPORTED_CHARS[style][:converter]
puts lowered_chars.map(&style_converter).join
