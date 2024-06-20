# Batch audio conversion script
#
# Pass it supported source and target extensions, and it will convert all files
# in the current directory. (I only use this occasionally, so it's unpolished.)
#
# https://codeconscious.github.io/2024/01/25/audio-conversion-ruby-script.html

SUPPORTED_EXTS = ['mp3', 'm4a', 'ogg']

source_ext = ARGV[0]
target_ext = ARGV[1]

def variables_invalid?(source_ext, target_ext, supported_exts)
  source_ext.nil? ||
    target_ext.nil? ||
    !supported_exts.include?(source_ext) ||
    !supported_exts.include?(target_ext)
end

if variables_invalid?(source_ext, target_ext, SUPPORTED_EXTS)
  puts 'Pass both (1) a source extension and (2) a target extension.'
  puts "Supported format: #{SUPPORTED_EXTS.join(', ')}."
  return
elsif source_ext == target_ext
  puts 'You cannot convert to the source format. Enter two different formats.'
  puts "Supported format: #{SUPPORTED_EXTS.join(', ')}."
  return
end

Dir.glob("*.#{source_ext}").each do |file|
  base_filename = File.basename(file, File.extname(file))

  case target_ext.downcase # TODO: Refactor to avoid this conditional on each iteration.
  when 'mp3'
    # `-vn` strips out the album art. The command does not work without it (at least for M4A).
    # TODO: Figure out how to copy over album art too. (Look into `-an`.)
    command = "ffmpeg -i \"#{file}\" -vn -ar 44100 -ac 2 -b:a 192k \"#{base_filename}.#{target_ext}\""
  when 'm4a'
    command = "ffmpeg -i \"#{file}\" -vn -ar 44100 -codec:a aac  \"#{base_filename}.#{target_ext}\""
  when 'ogg'
    command = "ffmpeg -i \"#{file}\" -vn -codec:a libvorbis -q:a 4 \"#{base_filename}.#{target_ext}\""
  end
  system(command)
end
