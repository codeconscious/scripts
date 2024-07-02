# Summary: When passed a directory, prints a CSV-formatted list of
# all of public methods defined in all Ruby files within that directory.

if ARGV.count != 1
  puts "Enter a single directory containing .rb files"
  return
end

dir = ARGV[0]

unless Dir.exist?(dir)
  puts "Directory does not exist."
  return
end

begin
  files = Dir.glob("#{dir}/**/*.rb")
rescue => ex
  puts "Error: #{ex.Message}"
  return
end

if files.count.zero?
  puts "No *.rb files found."
  return
end

puts "Directory,Filename,Line,Method"

MethodDefinition = Struct.new(:dir, :filename, :line_number, :method_name)
method_def_regex = /^\s+def ([\w_\?]+)/ # Method names are in capture group 0.

files.each do |file|
  line_number = 0
  method_defs = []

  IO.foreach(file) do |line|
    line_number = line_number + 1
    break if line.match? "^\s+?private"

    if (method_name = method_def_regex.match line)
      method_defs << MethodDefinition.new(
                       File.dirname(file),
                       File.basename(file),
                       line_number,
                       method_name.captures[0])
    end
  end

  method_defs.each do |m|
    puts "#{m.dir},#{m.filename},#{m.line_number},#{m.method_name}"
  end
end
